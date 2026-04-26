using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SemanticMemoryDedupIndexV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "ix_semantic_memories_user_key_domain_active_pending"
                ON semantic_memories ("UserId", lower("Key"), coalesce("Domain", ''))
                WHERE "Status" IN (1, 4);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "ix_semantic_memories_user_key_domain_active_pending";
                """);
        }
    }
}
