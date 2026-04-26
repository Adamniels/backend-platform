using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProceduralMemoryAuthorityV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AuthorityWeight",
                table: "procedural_rules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.9);

            migrationBuilder.AddColumn<long>(
                name: "ApprovedProceduralRuleId",
                table: "memory_review_queue",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_memory_review_queue_ApprovedProceduralRuleId",
                table: "memory_review_queue",
                column: "ApprovedProceduralRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_memory_review_queue_procedural_rules_ApprovedProceduralRule~",
                table: "memory_review_queue",
                column: "ApprovedProceduralRuleId",
                principalTable: "procedural_rules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_memory_review_queue_procedural_rules_ApprovedProceduralRule~",
                table: "memory_review_queue");

            migrationBuilder.DropIndex(
                name: "IX_memory_review_queue_ApprovedProceduralRuleId",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "AuthorityWeight",
                table: "procedural_rules");

            migrationBuilder.DropColumn(
                name: "ApprovedProceduralRuleId",
                table: "memory_review_queue");
        }
    }
}
