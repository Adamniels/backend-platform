using Platform.Domain.Features.Memory;

namespace Platform.Domain.Features.Memory.Entities;

/// <summary>
/// One row per user: highest-authority profile memory (user-entered). Stored with typed PostgreSQL columns
/// (<c>text[]</c> and <c>jsonb</c>). Inference and background jobs must not call <see cref="ApplyUserUpdate" /> — they
/// should use <see cref="MemoryItem" /> or <see cref="SemanticMemory" /> instead.
/// </summary>
public sealed class ExplicitUserProfile
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public MemoryUser? User { get; set; }

    public List<string> CoreInterests { get; set; } = new();
    public List<string> SecondaryInterests { get; set; } = new();
    public List<string> Goals { get; set; } = new();
    public string PreferencesJson { get; set; } = "[]";
    public string ActiveProjectsJson { get; set; } = "[]";
    public string SkillLevelsJson { get; set; } = "[]";

    public double AuthorityWeight { get; set; } = ExplicitUserProfileContent.ExplicitUserAuthorityValue;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public void ApplyUserUpdate(
        IReadOnlyList<string> coreInterests,
        IReadOnlyList<string> secondaryInterests,
        IReadOnlyList<string> goals,
        string preferencesJson,
        string activeProjectsJson,
        string skillLevelsJson,
        DateTimeOffset at)
    {
        ExplicitUserProfileContent.ThrowIfStringListInvalid(
            coreInterests,
            nameof(coreInterests),
            ExplicitUserProfileContent.MaxListSize);
        ExplicitUserProfileContent.ThrowIfStringListInvalid(
            secondaryInterests,
            nameof(secondaryInterests),
            ExplicitUserProfileContent.MaxListSize);
        ExplicitUserProfileContent.ThrowIfStringListInvalid(
            goals,
            nameof(goals),
            ExplicitUserProfileContent.MaxListSize);

        var prefs = ExplicitUserProfileContent.ParseAndValidatePreferencesJson(preferencesJson, nameof(preferencesJson));
        var projects = ExplicitUserProfileContent.ParseAndValidateActiveProjectsJson(
            activeProjectsJson,
            nameof(activeProjectsJson));
        var skills = ExplicitUserProfileContent.ParseAndValidateSkillLevelsJson(skillLevelsJson, nameof(skillLevelsJson));

        CoreInterests = coreInterests.Select(s => s.Trim()).ToList();
        SecondaryInterests = secondaryInterests.Select(s => s.Trim()).ToList();
        Goals = goals.Select(s => s.Trim()).ToList();
        PreferencesJson = ExplicitUserProfileContent.SerialisePreferencesJson(prefs);
        ActiveProjectsJson = ExplicitUserProfileContent.SerialiseProjectsJson(projects);
        SkillLevelsJson = ExplicitUserProfileContent.SerialiseSkillLevelsJson(skills);
        AuthorityWeight = ExplicitUserProfileContent.ExplicitUserAuthorityValue;
        UpdatedAt = at;
    }
}
