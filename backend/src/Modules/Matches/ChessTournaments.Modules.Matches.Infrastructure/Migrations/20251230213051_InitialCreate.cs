using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Matches.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "Matches");

            migrationBuilder.CreateTable(
                name: "Matches",
                schema: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WhitePlayerId = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    BlackPlayerId = table.Column<string>(
                        type: "nvarchar(256)",
                        maxLength: 256,
                        nullable: false
                    ),
                    BoardNumber = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Moves = table.Column<string>(
                        type: "nvarchar(max)",
                        maxLength: 10000,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "MatchTags",
                schema: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchTags_Matches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "Matches",
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_BlackPlayerId",
                schema: "Matches",
                table: "Matches",
                column: "BlackPlayerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_IsCompleted",
                schema: "Matches",
                table: "Matches",
                column: "IsCompleted"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RoundId",
                schema: "Matches",
                table: "Matches",
                column: "RoundId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentId",
                schema: "Matches",
                table: "Matches",
                column: "TournamentId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Matches_WhitePlayerId",
                schema: "Matches",
                table: "Matches",
                column: "WhitePlayerId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MatchTags_MatchId",
                schema: "Matches",
                table: "MatchTags",
                column: "MatchId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MatchTags_MatchId_Name",
                schema: "Matches",
                table: "MatchTags",
                columns: new[] { "MatchId", "Name" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_MatchTags_Name",
                schema: "Matches",
                table: "MatchTags",
                column: "Name"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MatchTags", schema: "Matches");

            migrationBuilder.DropTable(name: "Matches", schema: "Matches");
        }
    }
}
