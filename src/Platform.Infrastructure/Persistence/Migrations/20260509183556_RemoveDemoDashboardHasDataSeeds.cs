using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDemoDashboardHasDataSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "InputNeededItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InputNeededItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "InputNeededItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MemoryInsights",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MemoryInsights",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MemoryInsights",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MemoryInsights",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MemoryInsights",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "NewsItems",
                keyColumn: "Id",
                keyValue: "n1");

            migrationBuilder.DeleteData(
                table: "NewsItems",
                keyColumn: "Id",
                keyValue: "n2");

            migrationBuilder.DeleteData(
                table: "SavedItems",
                keyColumn: "Id",
                keyValue: "sv1");

            migrationBuilder.DeleteData(
                table: "WorkflowRuns",
                keyColumn: "Id",
                keyValue: "wr1");

            migrationBuilder.DeleteData(
                table: "WorkflowRuns",
                keyColumn: "Id",
                keyValue: "wr2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                table: "SavedItems",
                columns: new[] { "Id", "Kind", "SavedAt", "Title" },
                values: new object[] { "sv1", "article", new DateTimeOffset(new DateTime(2026, 4, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Saved article (placeholder)" });

            migrationBuilder.InsertData(
                table: "WorkflowRuns",
                columns: new[] { "Id", "Name", "Status", "TemporalWorkflowId", "UpdatedAt" },
                values: new object[,]
                {
                    { "wr1", "News intelligence", 1, null, new DateTimeOffset(new DateTime(2026, 4, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { "wr2", "Side learning enrichment", 2, null, new DateTimeOffset(new DateTime(2026, 4, 1, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });
        }
    }
}
