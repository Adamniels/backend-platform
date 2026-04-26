using Microsoft.EntityFrameworkCore;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Persistence;

public static class MemoryV1EfConfiguration
{
    public static void ConfigureMemoryV1(this ModelBuilder modelBuilder)
    {
        var seedT = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<MemoryUser>(e =>
        {
            e.ToTable("memory_users");
            e.HasKey(x => x.Id);
            e.HasData(new MemoryUser { Id = MemoryUser.DefaultId, CreatedAt = seedT });
        });

        modelBuilder.Entity<ExplicitUserProfile>(e =>
        {
            e.ToTable("memory_explicit_profile");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UserId).IsUnique();
            e.Property(x => x.CoreInterests)
                .HasColumnType("text[]");
            e.Property(x => x.SecondaryInterests)
                .HasColumnType("text[]");
            e.Property(x => x.Goals)
                .HasColumnType("text[]");
            e.Property(x => x.PreferencesJson).HasColumnType("jsonb");
            e.Property(x => x.ActiveProjectsJson).HasColumnType("jsonb");
            e.Property(x => x.SkillLevelsJson).HasColumnType("jsonb");
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MemoryItem>(e =>
        {
            e.ToTable("memory_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(512);
            e.Property(x => x.Content).HasColumnType("text");
            e.Property(x => x.StructuredJson).HasColumnType("jsonb");
            e.Property(x => x.SourceType).HasMaxLength(64);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_memory_items_user_id");
            e.HasIndex(x => new { x.UserId, x.Status }).HasDatabaseName("ix_memory_items_user_id_status");
            e.HasIndex(x => new { x.UserId, x.MemoryType }).HasDatabaseName("ix_memory_items_user_id_memory_type");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_memory_items_created_at");
        });

        modelBuilder.Entity<MemoryEvent>(e =>
        {
            e.ToTable("memory_events");
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).HasMaxLength(256);
            e.Property(x => x.Domain).HasMaxLength(256);
            e.Property(x => x.WorkflowId).HasMaxLength(256);
            e.Property(x => x.ProjectId).HasMaxLength(256);
            e.Property(x => x.PayloadJson).HasColumnType("jsonb");

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_memory_events_user_id");
            e.HasIndex(x => new { x.UserId, x.OccurredAt }).HasDatabaseName("ix_memory_events_user_id_occurred_at");
            e.HasIndex(x => new { x.UserId, x.EventType }).HasDatabaseName("ix_memory_events_user_id_event_type");
        });

        modelBuilder.Entity<SemanticMemory>(e =>
        {
            e.ToTable("semantic_memories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(256);
            e.Property(x => x.Claim).HasColumnType("text");
            e.Property(x => x.Domain).HasMaxLength(256);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_semantic_memories_user_id");
            e.HasIndex(x => new { x.UserId, x.Status }).HasDatabaseName("ix_semantic_memories_user_id_status");
            e.HasIndex(x => new { x.UserId, x.Key }).HasDatabaseName("ix_semantic_memories_user_id_key");
        });

        modelBuilder.Entity<MemoryEvidence>(e =>
        {
            e.ToTable("memory_evidence");
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(2048);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SemanticMemory)
                .WithMany()
                .HasForeignKey(x => x.SemanticMemoryId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.SourceEvent)
                .WithMany()
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.UserId).HasDatabaseName("ix_memory_evidence_user_id");
            e.HasIndex(x => x.SemanticMemoryId).HasDatabaseName("ix_memory_evidence_semantic_memory_id");
            e.HasIndex(x => x.EventId).HasDatabaseName("ix_memory_evidence_event_id");
            e.HasIndex(x => new { x.UserId, x.SemanticMemoryId }).HasDatabaseName("ix_memory_evidence_user_semantic");
        });

        modelBuilder.Entity<ProceduralRule>(e =>
        {
            e.ToTable("procedural_rules");
            e.HasKey(x => x.Id);
            e.Property(x => x.WorkflowType).HasMaxLength(128);
            e.Property(x => x.RuleName).HasMaxLength(256);
            e.Property(x => x.RuleContent).HasColumnType("text");
            e.Property(x => x.Source).HasMaxLength(512);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.UserId, x.WorkflowType, x.Status })
                .HasDatabaseName("ix_procedural_rules_user_workflow_status");
            e.HasIndex(x => new { x.UserId, x.WorkflowType, x.RuleName, x.Version })
                .IsUnique()
                .HasDatabaseName("ix_procedural_rules_user_rule_name_version");
        });

        modelBuilder.Entity<MemoryReviewQueueItem>(e =>
        {
            e.ToTable("memory_review_queue");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(512);
            e.Property(x => x.Summary).HasMaxLength(4000);
            e.Property(x => x.ProposedChangeJson).HasColumnType("jsonb");
            e.Property(x => x.EvidenceJson).HasColumnType("jsonb");
            e.Property(x => x.RejectedReason).HasMaxLength(2000);
            e.Property(x => x.ReviewNotes).HasMaxLength(4000);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovedSemanticMemory)
                .WithMany()
                .HasForeignKey(x => x.ApprovedSemanticMemoryId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(x => new { x.UserId, x.Status, x.Priority })
                .HasDatabaseName("ix_memory_review_queue_user_id_status_priority");
            e.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_memory_review_queue_created_at");
        });

        modelBuilder.Entity<MemoryConsolidationRun>(e =>
        {
            e.ToTable("memory_consolidation_runs");
            e.HasKey(x => x.Id);
            e.Property(x => x.IdempotencyKey).HasMaxLength(256);
            e.Property(x => x.Error).HasMaxLength(8000);
            e.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasDatabaseName("ix_memory_consolidation_runs_idempotency_key");
            e.HasIndex(x => new { x.UserId, x.StartedAt })
                .HasDatabaseName("ix_memory_consolidation_runs_user_started");
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MemoryRelationship>(e =>
        {
            e.ToTable("memory_relationships");
            e.HasKey(x => x.Id);
            e.Property(x => x.FromEntity).HasMaxLength(512);
            e.Property(x => x.ToEntity).HasMaxLength(512);
            e.Property(x => x.Source).HasMaxLength(512);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.UserId, x.FromEntity }).HasDatabaseName("ix_memory_relationships_user_id_from");
            e.HasIndex(x => new { x.UserId, x.ToEntity }).HasDatabaseName("ix_memory_relationships_user_id_to");
            e.HasIndex(x => new { x.UserId, x.RelationType })
                .HasDatabaseName("ix_memory_relationships_user_id_relation_type");
        });
    }
}
