using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewsEmbeddingsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_item_embeddings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NewsItemId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmbeddingModelKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Dimensions = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    EmbeddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_item_embeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_item_embeddings_NewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "NewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "news_user_profiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LongTermEmbedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    SeedText = table.Column<string>(type: "text", nullable: false),
                    SeededAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_user_profiles", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "ix_news_item_embeddings_item_model",
                table: "news_item_embeddings",
                columns: new[] { "NewsItemId", "EmbeddingModelKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_item_embeddings");

            migrationBuilder.DropTable(
                name: "news_user_profiles");
        }
    }
}
