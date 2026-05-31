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
    public DbSet<NewsItemEmbedding> NewsItemEmbeddings => Set<NewsItemEmbedding>();
    public DbSet<NewsUserProfile> NewsUserProfiles => Set<NewsUserProfile>();
    public DbSet<NewsInteraction> NewsInteractions => Set<NewsInteraction>();
    public DbSet<NewsRankedFeed> NewsRankedFeeds => Set<NewsRankedFeed>();
    public DbSet<SideLearningSession> SideLearningSessions => Set<SideLearningSession>();
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
                DisplayName = "Operator",
                Email = "",
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
                DigestEmail = false,
            });
        });

        modelBuilder.Entity<WorkflowRun>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Name).HasMaxLength(512);
            e.Property(x => x.TemporalWorkflowId).HasMaxLength(256);
        });

        modelBuilder.Entity<NewsItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(1024);
            e.Property(x => x.Source).HasMaxLength(256);
            e.Property(x => x.Url).HasMaxLength(4096);
            e.Property(x => x.UrlHash).HasMaxLength(64);
            e.Property(x => x.Body).HasColumnType("text");
            e.Property(x => x.Author).HasMaxLength(512);
            e.Property(x => x.SourceFeedUrl).HasMaxLength(2048);
            e.HasIndex(x => x.UrlHash).IsUnique();
        });

        modelBuilder.Entity<SideLearningSession>(e =>
        {
            e.ToTable("side_learning_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.InitialPrompt).HasMaxLength(4096);
            e.Property(x => x.SelectedTopicTitle).HasMaxLength(512);
            e.Property(x => x.SelectedTopicReason).HasMaxLength(4096);
            e.Property(x => x.ReflectionText).HasMaxLength(16384);
            e.Property(x => x.WorkflowRunId).HasMaxLength(64);
            e.Property(x => x.TopicProposalsJson).HasColumnType("jsonb");
            e.Property(x => x.SessionContentJson).HasColumnType("jsonb");
            e.Property(x => x.SectionsProgressJson).HasColumnType("jsonb");
            e.HasIndex(x => new { x.UserId, x.CreatedAt });
            e.HasOne<MemoryUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SavedItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasMaxLength(64);
            e.Property(x => x.Title).HasMaxLength(1024);
            e.Property(x => x.Kind).HasMaxLength(32);
        });

        modelBuilder.Entity<MemoryInsight>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Label).HasMaxLength(256);
            e.Property(x => x.Content).HasMaxLength(4096);
        });

        modelBuilder.Entity<InputNeededItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).HasMaxLength(512);
            e.Property(x => x.Type).HasMaxLength(64);
            e.Property(x => x.Detail).HasMaxLength(4096);
        });

        modelBuilder.Entity<StatsSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Json).HasColumnType("TEXT");
            e.HasData(new StatsSnapshot
            {
                Id = StatsSnapshot.SingletonKey,
                Json = """{"tiles":[],"progress":[],"activity":[]}""",
            });
        });

        modelBuilder.Entity<NewsItemEmbedding>(e =>
        {
            e.ToTable("news_item_embeddings");
            e.HasKey(x => x.Id);
            e.Property(x => x.NewsItemId).HasMaxLength(64);
            e.Property(x => x.EmbeddingModelKey).HasMaxLength(256);
            e.Property(x => x.Embedding)
                .HasColumnType("vector(1536)");
            e.HasIndex(x => new { x.NewsItemId, x.EmbeddingModelKey })
                .IsUnique()
                .HasDatabaseName("ix_news_item_embeddings_item_model");
            e.HasOne(x => x.NewsItem)
                .WithMany()
                .HasForeignKey(x => x.NewsItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NewsUserProfile>(e =>
        {
            e.ToTable("news_user_profiles");
            e.HasKey(x => x.UserId);
            e.Property(x => x.SeedText).HasColumnType("text");
            e.Property(x => x.LongTermEmbedding)
                .HasColumnType("vector(1536)");
            e.Property(x => x.ShortTermEmbedding)
                .HasColumnType("vector(1536)")
                .IsRequired(false);
            e.Property(x => x.ActiveContextEmbedding)
                .HasColumnType("vector(1536)")
                .IsRequired(false);
        });

        modelBuilder.Entity<NewsRankedFeed>(e =>
        {
            e.ToTable("news_ranked_feeds");
            e.HasKey(x => x.UserId);
            e.Property(x => x.EntriesJson).HasColumnType("jsonb");
            e.Property(x => x.ModelUsed).HasMaxLength(128);
        });

        modelBuilder.Entity<NewsInteraction>(e =>
        {
            e.ToTable("news_interactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.NewsItemId).HasMaxLength(64);
            // Restrict delete — keep interaction history even when the article is removed.
            e.HasOne<NewsItem>()
                .WithMany()
                .HasForeignKey(x => x.NewsItemId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.UserId, x.RecordedAt })
                .HasDatabaseName("ix_news_interactions_user_recorded");
            e.HasIndex(x => x.NewsItemId)
                .HasDatabaseName("ix_news_interactions_item");
        });

        modelBuilder.ConfigureMemoryV1();
    }
}
