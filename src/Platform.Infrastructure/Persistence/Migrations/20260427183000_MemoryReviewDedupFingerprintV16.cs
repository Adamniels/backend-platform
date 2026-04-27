using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(PlatformDbContext))]
    [Migration("20260427183000_MemoryReviewDedupFingerprintV16")]
    public partial class MemoryReviewDedupFingerprintV16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DedupFingerprint",
                table: "memory_review_queue",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_memory_review_queue_pending_dedup",
                table: "memory_review_queue",
                columns: new[] { "UserId", "ProposalType", "DedupFingerprint" },
                unique: true,
                filter: "\"Status\" = 0 AND \"DedupFingerprint\" IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_memory_review_queue_pending_dedup",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "DedupFingerprint",
                table: "memory_review_queue");
        }
    }
}
