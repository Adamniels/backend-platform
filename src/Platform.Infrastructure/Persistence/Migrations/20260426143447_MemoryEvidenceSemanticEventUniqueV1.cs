using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryEvidenceSemanticEventUniqueV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_semantic_event_unique",
                table: "memory_evidence",
                columns: new[] { "SemanticMemoryId", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_memory_evidence_semantic_event_unique",
                table: "memory_evidence");
        }
    }
}
