using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewsItemIngestV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "NewsItems",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "NewsItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceFeedUrl",
                table: "NewsItems",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "NewsItems",
                type: "character varying(4096)",
                maxLength: 4096,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UrlHash",
                table: "NewsItems",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE "NewsItems"
                SET "UrlHash" = encode(sha256(convert_to("Id" || '|legacy-news', 'UTF8')), 'hex')
                WHERE "UrlHash" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_UrlHash",
                table: "NewsItems",
                column: "UrlHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsItems_UrlHash",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "SourceFeedUrl",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "UrlHash",
                table: "NewsItems");
        }
    }
}
