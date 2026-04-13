using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialTournamentRequestsCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "TournamentRequests");

            migrationBuilder.CreateTable(
                name: "TournamentRequests",
                schema: "TournamentRequests",
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
                    RequestedStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    RequestedBy = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: false
                    ),
                    Status = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    ReviewedBy = table.Column<string>(
                        type: "nvarchar(450)",
                        maxLength: 450,
                        nullable: true
                    ),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(
                        type: "nvarchar(1000)",
                        maxLength: 1000,
                        nullable: true
                    ),
                    CreatedTournamentId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: true
                    ),
                    Format = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    TimeControl = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    TimeInMinutes = table.Column<int>(type: "int", nullable: false),
                    IncrementInSeconds = table.Column<int>(type: "int", nullable: false),
                    NumberOfRounds = table.Column<int>(type: "int", nullable: false),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    MinPlayers = table.Column<int>(type: "int", nullable: false),
                    AllowByes = table.Column<bool>(type: "bit", nullable: false),
                    EntryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentRequests", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_CreatedAt",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "CreatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_RequestedBy",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "RequestedBy"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_RequestedStartDate",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "RequestedStartDate"
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_Status",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "Status"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TournamentRequests", schema: "TournamentRequests");
        }
    }
}
