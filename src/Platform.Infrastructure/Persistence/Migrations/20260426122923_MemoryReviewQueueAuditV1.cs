using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryReviewQueueAuditV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ApprovedSemanticMemoryId",
                table: "memory_review_queue",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "memory_review_queue",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedAt",
                table: "memory_review_queue",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "memory_review_queue",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_memory_review_queue_ApprovedSemanticMemoryId",
                table: "memory_review_queue",
                column: "ApprovedSemanticMemoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_memory_review_queue_semantic_memories_ApprovedSemanticMemor~",
                table: "memory_review_queue",
                column: "ApprovedSemanticMemoryId",
                principalTable: "semantic_memories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_memory_review_queue_semantic_memories_ApprovedSemanticMemor~",
                table: "memory_review_queue");

            migrationBuilder.DropIndex(
                name: "IX_memory_review_queue_ApprovedSemanticMemoryId",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "ApprovedSemanticMemoryId",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "memory_review_queue");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "memory_review_queue");
        }
    }
}
