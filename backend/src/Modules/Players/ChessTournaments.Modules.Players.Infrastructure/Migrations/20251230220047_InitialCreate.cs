using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Players.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "Players");

            migrationBuilder.CreateTable(
                name: "Players",
                schema: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: false
                    ),
                    FirstName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    LastName = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    Country = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Bio = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                    AvatarUrl = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    PeakRating = table.Column<int>(type: "int", nullable: false),
                    PeakRatingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalGamesPlayed = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Losses = table.Column<int>(type: "int", nullable: false),
                    Draws = table.Column<int>(type: "int", nullable: false),
                    TournamentsParticipated = table.Column<int>(type: "int", nullable: false),
                    TournamentsWon = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Players_UserId",
                schema: "Players",
                table: "Players",
                column: "UserId",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Players", schema: "Players");
        }
    }
}
