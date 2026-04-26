using Microsoft.EntityFrameworkCore;
using Platform.Domain.Features.Dashboard;
using Platform.Domain.Features.HumanInput;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Domain.Features.News;
using Platform.Domain.Features.Profile;
using Platform.Domain.Features.SavedItems;
using Platform.Domain.Features.SideLearning;
using Platform.Domain.Features.WorkflowRuns;

namespace Platform.Infrastructure.Persistence;

public sealed class PlatformDbContext(DbContextOptions<PlatformDbContext> options) : DbContext(options)
{
    public DbSet<PlatformProfile> Profiles => Set<PlatformProfile>();
    public DbSet<PlatformUserSettings> UserSettings => Set<PlatformUserSettings>();
    public DbSet<WorkflowRun> WorkflowRuns => Set<WorkflowRun>();
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    public DbSet<SideLearningTopic> SideLearningTopics => Set<SideLearningTopic>();
    public DbSet<SavedItem> SavedItems => Set<SavedItem>();
    public DbSet<MemoryInsight> MemoryInsights => Set<MemoryInsight>();
    public DbSet<MemoryUser> MemoryUsers => Set<MemoryUser>();
    public DbSet<MemoryItem> MemoryItems => Set<MemoryItem>();
    public DbSet<MemoryEvent> MemoryEvents => Set<MemoryEvent>();
    public DbSet<SemanticMemory> SemanticMemories => Set<SemanticMemory>();
    public DbSet<MemoryEvidence> MemoryEvidences => Set<MemoryEvidence>();
    public DbSet<ProceduralRule> ProceduralRules => Set<ProceduralRule>();
    public DbSet<MemoryReviewQueueItem> MemoryReviewQueueItems => Set<MemoryReviewQueueItem>();
    public DbSet<MemoryConsolidationRun> MemoryConsolidationRuns => Set<MemoryConsolidationRun>();
    public DbSet<MemoryRelationship> MemoryRelationships => Set<MemoryRelationship>();
    public DbSet<ExplicitUserProfile> ExplicitUserProfiles => Set<ExplicitUserProfile>();
    public DbSet<MemoryEmbedding> MemoryEmbeddings => Set<MemoryEmbedding>();
    public DbSet<InputNeededItem> InputNeededItems => Set<InputNeededItem>();
    public DbSet<StatsSnapshot> StatsSnapshots => Set<StatsSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlatformProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayName).HasMaxLength(256);
            e.Property(x => x.Email).HasMaxLength(512);
            e.HasData(new PlatformProfile
            {
                Id = PlatformProfile.SingletonKey,
                DisplayName = "You",
                Email = "you@example.com",
            });
        });

        modelBuilder.Entity<PlatformUserSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Theme).HasMaxLength(32);
            e.HasData(new PlatformUserSettings
            {
                Id = PlatformUserSettings.SingletonKey,
                Theme = "system",
                DigestEmail = true,
            });
        });

        modelBuilder.Entity<WorkflowRun>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(512);
            e.Property(x => x.TemporalWorkflowId).HasMaxLength(256);
            var t0 = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);
            e.HasData(
                new WorkflowRun { Id = "wr1", Name = "News intelligence", Status = WorkflowRunStatus.Running, UpdatedAt = t0, TemporalWorkflowId = null },
                new WorkflowRun { Id = "wr2", Name = "Side learning enrichment", Status = WorkflowRunStatus.NeedsInput, UpdatedAt = t0, TemporalWorkflowId = null });
        });

        modelBuilder.Entity<NewsItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(1024);
            e.Property(x => x.Source).HasMaxLength(256);
            var t = new DateTimeOffset(2026, 4, 2, 8, 0, 0, TimeSpan.Zero);
            e.HasData(
                new NewsItem { Id = "n1", Title = "Sample headline (placeholder)", Source = "Wire", PublishedAt = t },
                new NewsItem { Id = "n2", Title = "Another story placeholder", Source = "Digest", PublishedAt = t });
        });

        modelBuilder.Entity<SideLearningTopic>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(512);
            e.HasData(
                new SideLearningTopic { Id = "s1", Title = "Foundations", ProgressPercent = 40 },
                new SideLearningTopic { Id = "s2", Title = "Applied practice", ProgressPercent = 10 });
        });

        modelBuilder.Entity<SavedItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(1024);
            e.Property(x => x.Kind).HasMaxLength(32);
            var t = new DateTimeOffset(2026, 4, 3, 10, 0, 0, TimeSpan.Zero);
            e.HasData(new SavedItem { Id = "sv1", Title = "Saved article (placeholder)", Kind = "article", SavedAt = t });
        });

        modelBuilder.Entity<MemoryInsight>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Label).HasMaxLength(256);
            e.Property(x => x.Content).HasMaxLength(4096);
            e.HasData(
                new MemoryInsight
                {
                    Id = 1,
                    Label = "Recurring Interest",
                    Content = "You consistently engage with AI governance and regulation content over the past 6 weeks.",
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
                    Content = "Your reading behavior suggests growing interest in hardware-level AI acceleration.",
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
                    Content = "Based on your interests, a learning path toward AI Safety Research would match your profile well.",
                    Strength = 82,
                    Confirmed = false,
                });
        });

        modelBuilder.Entity<InputNeededItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).HasMaxLength(512);
            e.Property(x => x.Type).HasMaxLength(64);
            e.Property(x => x.Detail).HasMaxLength(4096);
            e.HasData(
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
                    Detail =
                        "Detected reading patterns suggesting interest in Quantum Computing. Add it to your interest profile?",
                },
                new InputNeededItem
                {
                    Id = 3,
                    Text = "Choose your next learning topic",
                    Type = "Choose",
                    Urgent = false,
                    Detail =
                        "You have completed your current track. Select a new area to explore from your recommended topics.",
                });
        });

        modelBuilder.Entity<StatsSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Json).HasColumnType("TEXT");
            e.HasData(new StatsSnapshot { Id = StatsSnapshot.SingletonKey, Json = StatsSeedJson.Value });
        });

        modelBuilder.ConfigureMemoryV1();
    }
}
