using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryBeliefGovernanceV15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FromEntityKind",
                table: "memory_relationships",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvenanceJson",
                table: "memory_relationships",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToEntityKind",
                table: "memory_relationships",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Polarity",
                table: "memory_evidence",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Support");

            migrationBuilder.AddColumn<string>(
                name: "ProvenanceJson",
                table: "memory_evidence",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReliabilityWeight",
                table: "memory_evidence",
                type: "double precision",
                nullable: false,
                defaultValue: 0.55);

            migrationBuilder.AddColumn<string>(
                name: "SchemaVersion",
                table: "memory_evidence",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "memory_evidence",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceKind",
                table: "memory_evidence",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "SystemHeuristic");

            migrationBuilder.CreateIndex(
                name: "ix_memory_evidence_user_polarity",
                table: "memory_evidence",
                columns: new[] { "UserId", "Polarity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_memory_evidence_user_polarity",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "FromEntityKind",
                table: "memory_relationships");

            migrationBuilder.DropColumn(
                name: "ProvenanceJson",
                table: "memory_relationships");

            migrationBuilder.DropColumn(
                name: "ToEntityKind",
                table: "memory_relationships");

            migrationBuilder.DropColumn(
                name: "Polarity",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "ProvenanceJson",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "ReliabilityWeight",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "SchemaVersion",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "memory_evidence");

            migrationBuilder.DropColumn(
                name: "SourceKind",
                table: "memory_evidence");
        }
    }
}
