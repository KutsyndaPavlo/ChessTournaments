using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "Tournaments");

            migrationBuilder.CreateTable(
                name: "Tournaments",
                schema: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Description = table.Column<string>(
                        type: "nvarchar(2000)",
                        maxLength: 2000,
                        nullable: false
                    ),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    Settings_Format = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    Settings_TimeControl = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    Settings_TimeInMinutes = table.Column<int>(type: "int", nullable: false),
                    Settings_IncrementInSeconds = table.Column<int>(type: "int", nullable: false),
                    Settings_NumberOfRounds = table.Column<int>(type: "int", nullable: false),
                    Settings_MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    Settings_MinPlayers = table.Column<int>(type: "int", nullable: false),
                    Settings_AllowByes = table.Column<bool>(type: "bit", nullable: false),
                    Settings_EntryFee = table.Column<decimal>(
                        type: "decimal(18,2)",
                        nullable: false
                    ),
                    OrganizerId = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: false
                    ),
                    Location = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Rounds",
                schema: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Tournaments",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "TournamentPlayers",
                schema: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: false
                    ),
                    PlayerName = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    TotalScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GamesPlayed = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPlayers_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "Tournaments",
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "RoundMatches",
                schema: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardNumber = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    RoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoundMatches_Rounds_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "Tournaments",
                        principalTable: "Rounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_RoundMatches_MatchId",
                schema: "Tournaments",
                table: "RoundMatches",
                column: "MatchId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RoundMatches_RoundId",
                schema: "Tournaments",
                table: "RoundMatches",
                column: "RoundId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_TournamentId_RoundNumber",
                schema: "Tournaments",
                table: "Rounds",
                columns: new[] { "TournamentId", "RoundNumber" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPlayers_TournamentId_PlayerId",
                schema: "Tournaments",
                table: "TournamentPlayers",
                columns: new[] { "TournamentId", "PlayerId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_OrganizerId",
                schema: "Tournaments",
                table: "Tournaments",
                column: "OrganizerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_StartDate",
                schema: "Tournaments",
                table: "Tournaments",
                column: "StartDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_Status",
                schema: "Tournaments",
                table: "Tournaments",
                column: "Status"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RoundMatches", schema: "Tournaments");

            migrationBuilder.DropTable(name: "TournamentPlayers", schema: "Tournaments");

            migrationBuilder.DropTable(name: "Rounds", schema: "Tournaments");

            migrationBuilder.DropTable(name: "Tournaments", schema: "Tournaments");
        }
    }
}
