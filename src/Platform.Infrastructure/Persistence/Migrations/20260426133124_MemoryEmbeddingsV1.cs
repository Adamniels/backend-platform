using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryEmbeddingsV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "memory_embeddings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MemoryItemId = table.Column<long>(type: "bigint", nullable: false),
                    EmbeddingModelKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmbeddingModelVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Dimensions = table.Column<int>(type: "integer", nullable: false),
                    ContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_embeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_embeddings_memory_items_MemoryItemId",
                        column: x => x.MemoryItemId,
                        principalTable: "memory_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_memory_embeddings_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_memory_embeddings_MemoryItemId",
                table: "memory_embeddings",
                column: "MemoryItemId");

            migrationBuilder.CreateIndex(
                name: "ix_memory_embeddings_user_item_model",
                table: "memory_embeddings",
                columns: new[] { "UserId", "MemoryItemId", "EmbeddingModelKey" },
                unique: true);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS ix_memory_embeddings_embedding_hnsw
                ON memory_embeddings USING hnsw ("Embedding" vector_cosine_ops)
                WITH (m = 16, ef_construction = 64);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_memory_embeddings_embedding_hnsw;");
            migrationBuilder.DropTable(
                name: "memory_embeddings");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
