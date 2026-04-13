using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Tournaments.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Tournaments",
                table: "Tournaments",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Tournaments",
                table: "Tournaments",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Tournaments",
                table: "Tournaments",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Tournaments",
                table: "TournamentPlayers",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Tournaments",
                table: "TournamentPlayers",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Tournaments",
                table: "TournamentPlayers",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Tournaments",
                table: "Rounds",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Tournaments",
                table: "Rounds",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Tournaments",
                table: "Rounds",
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
                schema: "Tournaments",
                table: "Tournaments"
            );

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Tournaments",
                table: "Tournaments"
            );

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Tournaments",
                table: "Tournaments"
            );

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Tournaments",
                table: "TournamentPlayers"
            );

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Tournaments",
                table: "TournamentPlayers"
            );

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Tournaments",
                table: "TournamentPlayers"
            );

            migrationBuilder.DropColumn(name: "DeletedAt", schema: "Tournaments", table: "Rounds");

            migrationBuilder.DropColumn(name: "IsDeleted", schema: "Tournaments", table: "Rounds");

            migrationBuilder.DropColumn(name: "RowVersion", schema: "Tournaments", table: "Rounds");
        }
    }
}
