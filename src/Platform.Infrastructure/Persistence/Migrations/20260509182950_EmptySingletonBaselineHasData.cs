using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EmptySingletonBaselineHasData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "", "" });

            migrationBuilder.InsertData(
                table: "StatsSnapshots",
                columns: new[] { "Id", "Json" },
                values: new object[] { 1, "{\"tiles\":[],\"progress\":[],\"activity\":[]}" });

            migrationBuilder.UpdateData(
                table: "UserSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "DigestEmail",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "StatsSnapshots",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DisplayName", "Email" },
                values: new object[] { "You", "you@example.com" });

            migrationBuilder.UpdateData(
                table: "UserSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "DigestEmail",
                value: true);
        }
    }
}
