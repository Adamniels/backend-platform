using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Features.Memory.Context;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Procedural;

public sealed class EfProceduralRuleService(PlatformDbContext db) : IProceduralRuleService
{
    public async Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> ListForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var uid = userId is 0 ? MemoryUser.DefaultId : userId;
        var rows = await db.ProceduralRules
            .AsNoTracking()
            .Where(x => x.UserId == uid)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.Select(ToSummary).ToList();
    }

    public async Task<ProceduralRuleDetailV1Dto?> GetDetailAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var uid = userId is 0 ? MemoryUser.DefaultId : userId;
        var r = await db.ProceduralRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid, cancellationToken)
            .ConfigureAwait(false);
        return r is null ? null : ToDetail(r);
    }

    public async Task<long> CreateAndActivateAsync(
        int userId,
        string workflowType,
        string ruleName,
        string ruleContent,
        int priority,
        string source,
        double authorityWeight,
        CancellationToken cancellationToken = default)
    {
        var uid = userId is 0 ? MemoryUser.DefaultId : userId;
        var auth = MemoryValueConstraints.Clamp01(authorityWeight);
        var at = DateTimeOffset.UtcNow;
        await using var tx = await db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var rule = ProceduralRule.CreateFirstVersion(
                uid,
                workflowType,
                ruleName,
                ruleContent,
                priority,
                source,
                auth,
                at);
            db.ProceduralRules.Add(rule);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await DeprecateActiveSiblingsAsync(uid, rule.WorkflowType, rule.RuleName, at, cancellationToken)
                .ConfigureAwait(false);
            rule.Activate(at);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return rule.Id;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<long> PublishNewVersionActivateAsync(
        long basisRuleId,
        int userId,
        string ruleContent,
        double? authorityWeight,
        CancellationToken cancellationToken = default)
    {
        var uid = userId is 0 ? MemoryUser.DefaultId : userId;
        var basis = await db.ProceduralRules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == basisRuleId && x.UserId == uid, cancellationToken)
            .ConfigureAwait(false);
        if (basis is null)
        {
            throw new MemoryDomainException("Procedural rule was not found for this user.");
        }

        var auth = MemoryValueConstraints.Clamp01(authorityWeight ?? basis.AuthorityWeight);
        var at = DateTimeOffset.UtcNow;
        await using var tx = await db.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);
        try
        {
            var latest = await db.ProceduralRules
                .Where(
                    x => x.UserId == uid &&
                        x.WorkflowType == basis.WorkflowType &&
                        x.RuleName == basis.RuleName)
                .OrderByDescending(x => x.Version)
                .FirstAsync(cancellationToken)
                .ConfigureAwait(false);
            var next = latest.NewVersionWithContent(ruleContent ?? "", latest.Version + 1, at, auth);
            db.ProceduralRules.Add(next);
            await DeprecateActiveSiblingsAsync(uid, next.WorkflowType, next.RuleName, at, cancellationToken)
                .ConfigureAwait(false);
            next.Activate(at);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return next.Id;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<ProceduralRule> SetPriorityAsync(
        long id,
        int userId,
        int priority,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        row.SetPriority(priority, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    public async Task<ProceduralRule> ActivateAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        var at = DateTimeOffset.UtcNow;
        await DeprecateActiveSiblingsAsync(row.UserId, row.WorkflowType, row.RuleName, at, cancellationToken)
            .ConfigureAwait(false);
        row.Activate(at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    public async Task<ProceduralRule> DeprecateAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var row = await LoadMutableAsync(id, userId, cancellationToken).ConfigureAwait(false);
        row.Deprecate(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return row;
    }

    public async Task<long> ApplyApprovedNewProceduralProposalAsync(
        int userId,
        NewProceduralRuleMemoryProposalV1 payload,
        DateTimeOffset at,
        CancellationToken cancellationToken = default)
    {
        var approvedAuth = AuthorityWeight.UserApprovedProcedural.Value;
        if (payload.BasisRuleId is null or 0)
        {
            var wf = payload.WorkflowType?.Trim() ?? "";
            var rn = payload.RuleName?.Trim() ?? "";
            var rule = ProceduralRule.CreateFirstVersion(
                userId,
                wf,
                rn,
                payload.RuleContent,
                payload.Priority,
                payload.Source.Trim(),
                approvedAuth,
                at);
            db.ProceduralRules.Add(rule);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await DeprecateActiveSiblingsAsync(userId, rule.WorkflowType, rule.RuleName, at, cancellationToken)
                .ConfigureAwait(false);
            rule.Activate(at);
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return rule.Id;
        }

        var basis = await db.ProceduralRules
            .FirstOrDefaultAsync(
                x => x.Id == payload.BasisRuleId && x.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        if (basis is null)
        {
            throw new MemoryDomainException("Basis procedural rule was not found for this user.");
        }

        var latest = await db.ProceduralRules
            .Where(
                x => x.UserId == userId &&
                    x.WorkflowType == basis.WorkflowType &&
                    x.RuleName == basis.RuleName)
            .OrderByDescending(x => x.Version)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);
        var next = latest.NewVersionWithContent(payload.RuleContent, latest.Version + 1, at, approvedAuth);
        if (!string.IsNullOrWhiteSpace(payload.Source))
        {
            next.SetProvenance(payload.Source, at);
        }

        db.ProceduralRules.Add(next);
        await DeprecateActiveSiblingsAsync(userId, next.WorkflowType, next.RuleName, at, cancellationToken)
            .ConfigureAwait(false);
        next.Activate(at);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return next.Id;
    }

    private static ProceduralRuleSummaryV1Dto ToSummary(ProceduralRule x) =>
        new(
            x.Id,
            x.WorkflowType,
            x.RuleName,
            x.Version,
            x.Priority,
            MemoryContextV1Scoring.ProceduralStatusString(x.Status),
            x.AuthorityWeight,
            x.Source,
            x.UpdatedAt);

    private static ProceduralRuleDetailV1Dto ToDetail(ProceduralRule x) =>
        new(
            x.Id,
            x.UserId,
            x.WorkflowType,
            x.RuleName,
            x.RuleContent,
            x.Priority,
            x.Source,
            x.AuthorityWeight,
            x.Version,
            MemoryContextV1Scoring.ProceduralStatusString(x.Status),
            x.CreatedAt,
            x.UpdatedAt);

    private async Task DeprecateActiveSiblingsAsync(
        int userId,
        string workflowType,
        string ruleName,
        DateTimeOffset at,
        CancellationToken cancellationToken)
    {
        var actives = await db.ProceduralRules
            .Where(
                x => x.UserId == userId &&
                    x.WorkflowType == workflowType &&
                    x.RuleName == ruleName &&
                    x.Status == ProceduralRuleStatus.Active)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        foreach (var r in actives)
        {
            r.Deprecate(at);
        }
    }

    private async Task<ProceduralRule> LoadMutableAsync(
        long id,
        int userId,
        CancellationToken cancellationToken)
    {
        var uid = userId is 0 ? MemoryUser.DefaultId : userId;
        var row = await db.ProceduralRules
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            throw new MemoryDomainException("Procedural rule was not found for this user.");
        }

        return row;
    }
}
