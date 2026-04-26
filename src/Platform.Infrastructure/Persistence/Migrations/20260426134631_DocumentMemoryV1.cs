using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DocumentMemoryV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_memory_embeddings_user_item_model",
                table: "memory_embeddings");

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "memory_items",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectId",
                table: "memory_items",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChunkIndex",
                table: "memory_embeddings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddedText",
                table: "memory_embeddings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_user_id_domain",
                table: "memory_items",
                columns: new[] { "UserId", "Domain" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_items_user_id_project_id",
                table: "memory_items",
                columns: new[] { "UserId", "ProjectId" });

            migrationBuilder.CreateIndex(
                name: "ix_memory_embeddings_user_item_model_chunk",
                table: "memory_embeddings",
                columns: new[] { "UserId", "MemoryItemId", "EmbeddingModelKey", "ChunkIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_memory_items_user_id_domain",
                table: "memory_items");

            migrationBuilder.DropIndex(
                name: "ix_memory_items_user_id_project_id",
                table: "memory_items");

            migrationBuilder.DropIndex(
                name: "ix_memory_embeddings_user_item_model_chunk",
                table: "memory_embeddings");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "memory_items");

            migrationBuilder.DropColumn(
                name: "ChunkIndex",
                table: "memory_embeddings");

            migrationBuilder.DropColumn(
                name: "EmbeddedText",
                table: "memory_embeddings");

            migrationBuilder.CreateIndex(
                name: "ix_memory_embeddings_user_item_model",
                table: "memory_embeddings",
                columns: new[] { "UserId", "MemoryItemId", "EmbeddingModelKey" },
                unique: true);
        }
    }
}
