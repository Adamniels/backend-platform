using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Domain.Features.Dashboard;
using Platform.Domain.Features.HumanInput;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.Memory.ValueObjects;
using Platform.Domain.Features.News;
using Platform.Domain.Features.Profile;
using Platform.Domain.Features.SavedItems;
using Platform.Domain.Features.SideLearning;
using Platform.Domain.Features.WorkflowRuns;
using Platform.Infrastructure.Persistence;

if (args.Contains("-h", StringComparer.Ordinal) || args.Contains("--help", StringComparer.Ordinal))
{
    Console.Error.WriteLine(
        """
        Reset the database (all application data) and insert mock data.

        Connection string (in order of precedence):
          1) first positional argument
          2) environment variable CONNECTIONSTRINGS__DEFAULT
          3) Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform

        Migrations table __EFMigrationsHistory is left intact (schema and migration state preserved).
        """);
    return;
}

var connectionString = args.FirstOrDefault(a => !a.StartsWith("-", StringComparison.Ordinal))
                       ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT")
                       ?? "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";

var options = new DbContextOptionsBuilder<PlatformDbContext>()
    .UseNpgsql(connectionString, o => o.UseVector())
    .Options;

await using var db = new PlatformDbContext(options);

Console.WriteLine("Truncating all public data tables (except __EFMigrationsHistory)…");
await TruncateAllDataTablesAsync(db, CancellationToken.None);

Console.WriteLine("Inserting mock data…");
await MockDatabaseSeed.SeedAsync(db, CancellationToken.None);
await db.SaveChangesAsync();

Console.WriteLine("Done.");
return;

static async Task TruncateAllDataTablesAsync(PlatformDbContext db, CancellationToken cancellationToken)
{
    // Single TRUNCATE of every user table: CASCADE clears FKs; RESTART IDENTITY keeps sequences clean.
    var tableNames = await GetPublicTableNamesAsync(db, cancellationToken);
    if (tableNames.Count == 0)
    {
        return;
    }

    const string history = "__EFMigrationsHistory";
    var toTruncate = tableNames.Where(n => n != history).ToList();
    if (toTruncate.Count == 0)
    {
        return;
    }

    var quoted = string.Join(", ", toTruncate.Select(QuoteRegclass));
    var sql = $"TRUNCATE TABLE {quoted} RESTART IDENTITY CASCADE";
    await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
}

static string QuoteRegclass(string identifier)
{
    return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

static async Task<IReadOnlyList<string>> GetPublicTableNamesAsync(
    PlatformDbContext db,
    CancellationToken cancellationToken)
{
    const string q =
        """
        SELECT c.relname
        FROM pg_class c
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'public'
          AND c.relkind = 'r'
        ORDER BY c.relname
        """;
    var conn = db.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open)
    {
        await conn.OpenAsync(cancellationToken);
    }

    await using var cmd = conn.CreateCommand();
    cmd.CommandText = q;
    var list = new List<string>();
    await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
    while (await r.ReadAsync(cancellationToken))
    {
        list.Add(r.GetString(0));
    }

    return list;
}

file static class MockDatabaseSeed
{
    private const string StatsJson =
        """
        {"tiles":[{"label":"Sessions Completed","value":24,"unit":"","color":"var(--accent)","sub":"+3 this week"},{"label":"Articles Read","value":187,"unit":"","color":"var(--accent)","sub":"+12 this week"},{"label":"Saved Items","value":43,"unit":"","color":"rgba(232,237,248,0.45)","sub":"5 added recently"},{"label":"Day Streak","value":12,"unit":"d","color":"#34d399","sub":"Personal best: 18d"},{"label":"Avg Session Length","value":38,"unit":"min","color":"var(--accent)","sub":"Target: 45 min"},{"label":"Topics Explored","value":9,"unit":"","color":"#a78bfa","sub":"3 in progress"},{"label":"Knowledge Score","value":94,"unit":"%","color":"#34d399","sub":"Top 8% of users"},{"label":"Hours Learned","value":41,"unit":"h","color":"#fbbf24","sub":"This month"}],"progress":[{"label":"Weekly Learning Goal","value":68,"color":"var(--accent)"},{"label":"AI Ethics Mastery","value":82,"color":"var(--accent)"},{"label":"Reading Streak","value":45,"color":"#34d399"},{"label":"Profile Completion","value":91,"color":"#a78bfa"}],"activity":[{"day":"Mon","sessions":2},{"day":"Tue","sessions":1},{"day":"Wed","sessions":3},{"day":"Thu","sessions":0},{"day":"Fri","sessions":2},{"day":"Sat","sessions":1},{"day":"Sun","sessions":2}]}
        """;

    public static async Task SeedAsync(PlatformDbContext db, CancellationToken cancellationToken)
    {
        var t0 = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
        var tNews = new DateTimeOffset(2026, 4, 2, 8, 0, 0, TimeSpan.Zero);
        var tSaved = new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.Zero);

        db.Add(new MemoryUser { Id = MemoryUser.DefaultId, CreatedAt = t0 });

        db.Add(
            new PlatformProfile
            {
                Id = PlatformProfile.SingletonKey,
                DisplayName = "You",
                Email = "you@example.com",
            });
        db.Add(
            new PlatformUserSettings
            {
                Id = PlatformUserSettings.SingletonKey,
                Theme = "system",
                DigestEmail = true,
            });
        db.Add(
            new WorkflowRun
            {
                Id = "wr1",
                Name = "News intelligence",
                Status = WorkflowRunStatus.Running,
                UpdatedAt = t0,
                TemporalWorkflowId = null,
            });
        db.Add(
            new WorkflowRun
            {
                Id = "wr2",
                Name = "Side learning enrichment",
                Status = WorkflowRunStatus.NeedsInput,
                UpdatedAt = t0,
                TemporalWorkflowId = null,
            });
        db.Add(
            new NewsItem
            {
                Id = "n1",
                Title = "Sample headline (placeholder)",
                Source = "Wire",
                PublishedAt = tNews,
            });
        db.Add(
            new NewsItem
            {
                Id = "n2",
                Title = "Another story placeholder",
                Source = "Digest",
                PublishedAt = tNews,
            });
        db.Add(new SideLearningTopic { Id = "s1", Title = "Foundations", ProgressPercent = 40 });
        db.Add(new SideLearningTopic { Id = "s2", Title = "Applied practice", ProgressPercent = 10 });
        db.Add(
            new SavedItem
            {
                Id = "sv1",
                Title = "Saved article (placeholder)",
                Kind = "article",
                SavedAt = tSaved,
            });
        AddMemoryInsights(db, t0);
        AddInputNeededItems(db);
        db.Add(new StatsSnapshot { Id = StatsSnapshot.SingletonKey, Json = StatsJson });

        await db.SaveChangesAsync(cancellationToken);

        var at = t0;
        var explicitProfile = new ExplicitUserProfile
        {
            UserId = MemoryUser.DefaultId,
            CreatedAt = at,
            UpdatedAt = at,
        };
        explicitProfile.ApplyUserUpdate(
            new[] { "AI safety", "Reliable distributed systems" },
            new[] { "Board games" },
            new[] { "Ship a production-grade memory platform" },
            "[]",
            "[]",
            "[]",
            at);
        db.Add(explicitProfile);
        await db.SaveChangesAsync(cancellationToken);

        var note = MemoryItem.CreateNew(
            MemoryUser.DefaultId,
            MemoryItemType.Note,
            "On-call rotation starts Monday",
            "I will be primary for the search team during the week of 2026-04-14.",
            "user",
            0.9,
            0.88,
            at);
        note.PromoteToActive(at);
        var profileFact = MemoryItem.CreateNew(
            MemoryUser.DefaultId,
            MemoryItemType.ProfileFact,
            "Prefers sessions under 45 minutes",
            "User does best in focused, shorter learning blocks with concrete exercises.",
            "user",
            0.95,
            0.9,
            at,
            domain: "learning");
        profileFact.PromoteToActive(at);
        db.AddRange(note, profileFact);
        await db.SaveChangesAsync(cancellationToken);

        var semantic1 = SemanticMemory.CreateInitial(
            MemoryUser.DefaultId,
            "learning.style",
            "The user learns best with structured sessions under 60 minutes.",
            0.82d,
            AuthorityWeight.Inferred,
            "learning",
            at);
        var semantic2 = SemanticMemory.CreateInitial(
            MemoryUser.DefaultId,
            "interest.governance",
            "The user frequently reads and saves content about AI governance and policy.",
            0.75d,
            AuthorityWeight.Inferred,
            "recommendation",
            at);
        db.AddRange(semantic1, semantic2);
        await db.SaveChangesAsync(cancellationToken);

        var event1 = MemoryEvent.Create(
            MemoryUser.DefaultId,
            "profile.updated",
            "Profile",
            null,
            "proj-mock-1",
            /*language=json*/
            """
            {"source": "dev-seed", "message": "Explicit profile was refreshed."}
            """,
            at,
            at);
        var event2 = MemoryEvent.Create(
            MemoryUser.DefaultId,
            "learning.session.completed",
            "Learning",
            "wf-mock-1",
            null,
            null,
            at,
            at);
        db.AddRange(event1, event2);
        await db.SaveChangesAsync(cancellationToken);

        var ev1 = MemoryEvidence.Link(
            MemoryUser.DefaultId,
            semantic1.Id,
            event1.Id,
            0.4d,
            "Reinforced by a profile update in dev seed",
            at);
        var ev2 = MemoryEvidence.Link(
            MemoryUser.DefaultId,
            semantic2.Id,
            event2.Id,
            0.25d,
            "Tied to a learning session event from dev seed",
            at);
        db.AddRange(ev1, ev2);
        var rule1 = ProceduralRule.CreateFirstVersion(
            MemoryUser.DefaultId,
            "memory.context",
            "citations_required",
            "When answering with memory recall, list the top supporting memory rows by title.",
            10,
            "dev-seed",
            0.6d,
            at);
        rule1.Activate(at);
        var rule2 = ProceduralRule.CreateFirstVersion(
            MemoryUser.DefaultId,
            "learning",
            "session_cap_minutes",
            "Suggest sessions capped at 45 minutes unless the user opts in to longer blocks.",
            5,
            "dev-seed",
            0.55d,
            at);
        rule2.Activate(at);
        db.AddRange(rule1, rule2);
        var review1 = MemoryReviewQueueItem.Propose(
            MemoryUser.DefaultId,
            MemoryReviewProposalType.NewSemantic,
            "Proposed: user prefers video over text for new topics",
            "Detected from a short burst of course completions. Approve to store as semantic memory.",
            /*language=json*/
            """
            { "proposedKey": "learning.media_pref", "claim": "The user may prefer short video for unfamiliar topics." }
            """,
            null,
            3,
            at);
        db.Add(review1);
        var winStart = at.AddDays(-1);
        var winEnd = at;
        var consolidation = new MemoryConsolidationRun
        {
            UserId = MemoryUser.DefaultId,
            IdempotencyKey = "dev-seed-consolidation-1",
            Status = MemoryConsolidationRunStatus.Completed,
            WindowStart = winStart,
            WindowEnd = winEnd,
            ProcessedEventsCount = 2,
            ProposalsCreatedCount = 1,
            AutoUpdatesCount = 0,
            StartedAt = at,
            CompletedAt = at,
            Error = null,
        };
        db.Add(consolidation);
        var rel = MemoryRelationship.Define(
            MemoryUser.DefaultId,
            "interest.governance",
            MemoryRelationshipType.Learning,
            "project.memory-platform",
            0.7d,
            "dev-seed",
            at);
        db.Add(rel);

        await db.SaveChangesAsync(cancellationToken);

        var embText = (note.Content + " " + profileFact.Content).Trim();
        var floatVec = DeterministicRecallEmbeddingGenerator.EmbedText(
            embText,
            MemoryVectorRecallConstants.EmbeddingDimensions);
        var vec = new Vector(floatVec);
        var sha = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(embText)));

        var embedding = new MemoryEmbedding
        {
            UserId = MemoryUser.DefaultId,
            MemoryItemId = note.Id,
            EmbeddingModelKey = DeterministicRecallEmbeddingGenerator.DefaultModelKey,
            EmbeddingModelVersion = "1",
            Dimensions = MemoryVectorRecallConstants.EmbeddingDimensions,
            ContentSha256 = sha,
            ChunkIndex = 0,
            EmbeddedText = embText,
            Embedding = vec,
            CreatedAt = at,
            UpdatedAt = at,
        };
        db.Add(embedding);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void AddMemoryInsights(PlatformDbContext db, DateTimeOffset t0)
    {
        db.AddRange(
            new MemoryInsight
            {
                Id = 1,
                Label = "Recurring Interest",
                Content =
                    "You consistently engage with AI governance and regulation content over the past 6 weeks.",
                Strength = 94,
                Confirmed = true,
            },
            new MemoryInsight
            {
                Id = 2,
                Label = "Learning Pattern",
                Content = "You prefer structured sessions under 60 minutes, with hands-on exercises.",
                Strength = 87,
                Confirmed = true,
            },
            new MemoryInsight
            {
                Id = 3,
                Label = "Emerging Trend",
                Content =
                    "Your reading behavior suggests growing interest in hardware-level AI acceleration.",
                Strength = 61,
                Confirmed = false,
            },
            new MemoryInsight
            {
                Id = 4,
                Label = "Knowledge Gap",
                Content = "Foundational probability and statistics appear underrepresented in your learning history.",
                Strength = 78,
                Confirmed = false,
            },
            new MemoryInsight
            {
                Id = 5,
                Label = "Recommended Path",
                Content =
                    "Based on your interests, a learning path toward AI Safety Research would match your profile well.",
                Strength = 82,
                Confirmed = false,
            });
    }

    private static void AddInputNeededItems(PlatformDbContext db)
    {
        db.AddRange(
            new InputNeededItem
            {
                Id = 1,
                Text = "Rate your last AI Ethics session",
                Type = "Rating",
                Urgent = true,
                Detail =
                    "How would you rate the difficulty and quality of your last session? This helps calibrate future recommendations.",
            },
            new InputNeededItem
            {
                Id = 2,
                Text = "Confirm new interest: Quantum Computing?",
                Type = "Confirm",
                Urgent = false,
                Detail = "Detected reading patterns suggesting interest in Quantum Computing. Add it to your interest profile?",
            },
            new InputNeededItem
            {
                Id = 3,
                Text = "Choose your next learning topic",
                Type = "Choose",
                Urgent = false,
                Detail = "You have completed your current track. Select a new area to explore from your recommended topics.",
            });
    }
}
