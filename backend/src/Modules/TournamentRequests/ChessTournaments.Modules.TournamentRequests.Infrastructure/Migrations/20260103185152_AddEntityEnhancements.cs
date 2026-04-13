using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );
        }
    }
}
