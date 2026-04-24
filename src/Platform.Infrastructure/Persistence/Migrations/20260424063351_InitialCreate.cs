using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InputNeededItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Urgent = table.Column<bool>(type: "boolean", nullable: false),
                    Detail = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InputNeededItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemoryInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Strength = table.Column<int>(type: "integer", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryInsights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavedItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SavedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SideLearningTopics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ProgressPercent = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SideLearningTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Theme = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DigestEmail = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowRuns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TemporalWorkflowId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowRuns", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InputNeededItems",
                columns: new[] { "Id", "Detail", "Text", "Type", "Urgent" },
                values: new object[,]
                {
                    { 1, "How would you rate the difficulty and quality of your last session? This helps calibrate future recommendations.", "Rate your last AI Ethics session", "Rating", true },
                    { 2, "Detected reading patterns suggesting interest in Quantum Computing. Add it to your interest profile?", "Confirm new interest: Quantum Computing?", "Confirm", false },
                    { 3, "You have completed your current track. Select a new area to explore from your recommended topics.", "Choose your next learning topic", "Choose", false }
                });

            migrationBuilder.InsertData(
                table: "MemoryInsights",
                columns: new[] { "Id", "Confirmed", "Content", "Label", "Strength" },
                values: new object[,]
                {
                    { 1, true, "You consistently engage with AI governance and regulation content over the past 6 weeks.", "Recurring Interest", 94 },
                    { 2, true, "You prefer structured sessions under 60 minutes, with hands-on exercises.", "Learning Pattern", 87 },
                    { 3, false, "Your reading behavior suggests growing interest in hardware-level AI acceleration.", "Emerging Trend", 61 },
                    { 4, false, "Foundational probability and statistics appear underrepresented in your learning history.", "Knowledge Gap", 78 },
                    { 5, false, "Based on your interests, a learning path toward AI Safety Research would match your profile well.", "Recommended Path", 82 }
                });

            migrationBuilder.InsertData(
                table: "NewsItems",
                columns: new[] { "Id", "PublishedAt", "Source", "Title" },
                values: new object[,]
                {
                    { "n1", new DateTimeOffset(new DateTime(2026, 4, 2, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Wire", "Sample headline (placeholder)" },
                    { "n2", new DateTimeOffset(new DateTime(2026, 4, 2, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Digest", "Another story placeholder" }
                });

            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "DisplayName", "Email" },
                values: new object[] { 1, "You", "you@example.com" });

            migrationBuilder.InsertData(
                table: "SavedItems",
                columns: new[] { "Id", "Kind", "SavedAt", "Title" },
                values: new object[] { "sv1", "article", new DateTimeOffset(new DateTime(2026, 4, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Saved article (placeholder)" });

            migrationBuilder.InsertData(
                table: "SideLearningTopics",
                columns: new[] { "Id", "ProgressPercent", "Title" },
                values: new object[,]
                {
                    { "s1", 40, "Foundations" },
                    { "s2", 10, "Applied practice" }
                });

            migrationBuilder.InsertData(
                table: "StatsSnapshots",
                columns: new[] { "Id", "Json" },
                values: new object[] { 1, "{\"tiles\":[{\"label\":\"Sessions Completed\",\"value\":24,\"unit\":\"\",\"color\":\"var(--accent)\",\"sub\":\"+3 this week\"},{\"label\":\"Articles Read\",\"value\":187,\"unit\":\"\",\"color\":\"var(--accent)\",\"sub\":\"+12 this week\"},{\"label\":\"Saved Items\",\"value\":43,\"unit\":\"\",\"color\":\"rgba(232,237,248,0.45)\",\"sub\":\"5 added recently\"},{\"label\":\"Day Streak\",\"value\":12,\"unit\":\"d\",\"color\":\"#34d399\",\"sub\":\"Personal best: 18d\"},{\"label\":\"Avg Session Length\",\"value\":38,\"unit\":\"min\",\"color\":\"var(--accent)\",\"sub\":\"Target: 45 min\"},{\"label\":\"Topics Explored\",\"value\":9,\"unit\":\"\",\"color\":\"#a78bfa\",\"sub\":\"3 in progress\"},{\"label\":\"Knowledge Score\",\"value\":94,\"unit\":\"%\",\"color\":\"#34d399\",\"sub\":\"Top 8% of users\"},{\"label\":\"Hours Learned\",\"value\":41,\"unit\":\"h\",\"color\":\"#fbbf24\",\"sub\":\"This month\"}],\"progress\":[{\"label\":\"Weekly Learning Goal\",\"value\":68,\"color\":\"var(--accent)\"},{\"label\":\"AI Ethics Mastery\",\"value\":82,\"color\":\"var(--accent)\"},{\"label\":\"Reading Streak\",\"value\":45,\"color\":\"#34d399\"},{\"label\":\"Profile Completion\",\"value\":91,\"color\":\"#a78bfa\"}],\"activity\":[{\"day\":\"Mon\",\"sessions\":2},{\"day\":\"Tue\",\"sessions\":1},{\"day\":\"Wed\",\"sessions\":3},{\"day\":\"Thu\",\"sessions\":0},{\"day\":\"Fri\",\"sessions\":2},{\"day\":\"Sat\",\"sessions\":1},{\"day\":\"Sun\",\"sessions\":2}]}" });

            migrationBuilder.InsertData(
                table: "UserSettings",
                columns: new[] { "Id", "DigestEmail", "Theme" },
                values: new object[] { 1, true, "system" });

            migrationBuilder.InsertData(
                table: "WorkflowRuns",
                columns: new[] { "Id", "Name", "Status", "TemporalWorkflowId", "UpdatedAt" },
                values: new object[,]
                {
                    { "wr1", "News intelligence", 1, null, new DateTimeOffset(new DateTime(2026, 4, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "wr2", "Side learning enrichment", 2, null, new DateTimeOffset(new DateTime(2026, 4, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InputNeededItems");

            migrationBuilder.DropTable(
                name: "MemoryInsights");

            migrationBuilder.DropTable(
                name: "NewsItems");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "SavedItems");

            migrationBuilder.DropTable(
                name: "SideLearningTopics");

            migrationBuilder.DropTable(
                name: "StatsSnapshots");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "WorkflowRuns");
        }
    }
}
