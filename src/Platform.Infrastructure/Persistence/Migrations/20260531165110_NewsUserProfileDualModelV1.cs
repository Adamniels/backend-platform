using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewsUserProfileDualModelV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "ActiveContextEmbedding",
                table: "news_user_profiles",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ActiveContextUpdatedAt",
                table: "news_user_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "ShortTermEmbedding",
                table: "news_user_profiles",
                type: "vector(1536)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ShortTermUpdatedAt",
                table: "news_user_profiles",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveContextEmbedding",
                table: "news_user_profiles");

            migrationBuilder.DropColumn(
                name: "ActiveContextUpdatedAt",
                table: "news_user_profiles");

            migrationBuilder.DropColumn(
                name: "ShortTermEmbedding",
                table: "news_user_profiles");

            migrationBuilder.DropColumn(
                name: "ShortTermUpdatedAt",
                table: "news_user_profiles");
        }
    }
}
