using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SideLearningSessionsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SideLearningTopics");

            migrationBuilder.CreateTable(
                name: "side_learning_sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    InitialPrompt = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    SelectedTopicTitle = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SelectedTopicReason = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    TopicProposalsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SessionContentJson = table.Column<string>(type: "jsonb", nullable: false),
                    SectionsProgressJson = table.Column<string>(type: "jsonb", nullable: false),
                    ReflectionText = table.Column<string>(type: "character varying(16384)", maxLength: 16384, nullable: true),
                    WorkflowRunId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_side_learning_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_side_learning_sessions_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_side_learning_sessions_UserId_CreatedAt",
                table: "side_learning_sessions",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "side_learning_sessions");

            migrationBuilder.CreateTable(
                name: "SideLearningTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SideLearningTopics", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SideLearningTopics",
                columns: new[] { "Id", "ProgressPercent", "Title" },
                values: new object[,]
                {
                    { "s1", 40, "Foundations" },
                    { "s2", 10, "Applied practice" }
                });
        }
    }
}
