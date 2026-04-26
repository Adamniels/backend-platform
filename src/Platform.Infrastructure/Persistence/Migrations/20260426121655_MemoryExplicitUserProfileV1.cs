using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MemoryExplicitUserProfileV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "memory_explicit_profile",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CoreInterests = table.Column<List<string>>(type: "text[]", nullable: false),
                    SecondaryInterests = table.Column<List<string>>(type: "text[]", nullable: false),
                    Goals = table.Column<List<string>>(type: "text[]", nullable: false),
                    PreferencesJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActiveProjectsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SkillLevelsJson = table.Column<string>(type: "jsonb", nullable: false),
                    AuthorityWeight = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memory_explicit_profile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memory_explicit_profile_memory_users_UserId",
                        column: x => x.UserId,
                        principalTable: "memory_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_memory_explicit_profile_UserId",
                table: "memory_explicit_profile",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "memory_explicit_profile");
        }
    }
}
