using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemorySystemV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "memory_users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "memory_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Domain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    WorkflowId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProjectId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_events_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memory_items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MemoryType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    StructuredJson = table.Column<string>(type: "jsonb", nullable: true),
                    SourceType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AuthorityWeight = table.Column<double>(type: "double precision", nullable: false),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Importance = table.Column<double>(type: "double precision", nullable: false),
                    FreshnessScore = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAccessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_items_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memory_relationships",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FromEntity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    RelationType = table.Column<int>(type: "integer", nullable: false),
                    ToEntity = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Strength = table.Column<double>(type: "double precision", nullable: false),
                    Source = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_relationships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_relationships_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memory_review_queue",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProposalType = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ProposedChangeJson = table.Column<string>(type: "jsonb", nullable: true),
                    EvidenceJson = table.Column<string>(type: "jsonb", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_review_queue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_review_queue_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "procedural_rules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    WorkflowType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RuleContent = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_procedural_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_procedural_rules_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "semantic_memories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Claim = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    AuthorityWeight = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSupportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semantic_memories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_semantic_memories_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memory_evidence",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SemanticMemoryId = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Strength = table.Column<double>(type: "double precision", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_evidence_memory_events_EventId",
                        column: x => x.EventId,
                        principalTable: "memory_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memory_evidence_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memory_evidence_semantic_memories_SemanticMemoryId",
                        column: x => x.SemanticMemoryId,
                        principalTable: "semantic_memories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "memory_users",
                columns: new[] { "Id", "CreatedAt" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "ix_memory_events_user_id",
                table: "memory_events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_events_user_id_event_type",
                table: "memory_events",
                columns: new[] { "UserId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_events_user_id_occurred_at",
                table: "memory_events",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_event_id",
                table: "memory_evidence",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_semantic_memory_id",
                table: "memory_evidence",
                column: "SemanticMemoryId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_user_id",
                table: "memory_evidence",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_user_semantic",
                table: "memory_evidence",
                columns: new[] { "UserId", "SemanticMemoryId" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_created_at",
                table: "memory_items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_user_id",
                table: "memory_items",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_user_id_memory_type",
                table: "memory_items",
                columns: new[] { "UserId", "MemoryType" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_user_id_status",
                table: "memory_items",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_relationships_user_id_from",
                table: "memory_relationships",
                columns: new[] { "UserId", "FromEntity" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_relationships_user_id_relation_type",
                table: "memory_relationships",
                columns: new[] { "UserId", "RelationType" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_relationships_user_id_to",
                table: "memory_relationships",
                columns: new[] { "UserId", "ToEntity" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_review_queue_created_at",
                table: "memory_review_queue",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_memory_review_queue_user_id_status_priority",
                table: "memory_review_queue",
                columns: new[] { "UserId", "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "ix_procedural_rules_user_rule_name_version",
                table: "procedural_rules",
                columns: new[] { "UserId", "WorkflowType", "RuleName", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_procedural_rules_user_workflow_status",
                table: "procedural_rules",
                columns: new[] { "UserId", "WorkflowType", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_semantic_memories_user_id",
                table: "semantic_memories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_semantic_memories_user_id_key",
                table: "semantic_memories",
                columns: new[] { "UserId", "Key" });

            migrationBuilder.CreateIndex(
                name: "ix_semantic_memories_user_id_status",
                table: "semantic_memories",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "memory_evidence");

            migrationBuilder.DropTable(
                name: "memory_items");

            migrationBuilder.DropTable(
                name: "memory_relationships");

            migrationBuilder.DropTable(
                name: "memory_review_queue");

            migrationBuilder.DropTable(
                name: "procedural_rules");

            migrationBuilder.DropTable(
                name: "memory_events");

            migrationBuilder.DropTable(
                name: "semantic_memories");

            migrationBuilder.DropTable(
                name: "memory_users");
        }
    }
}
