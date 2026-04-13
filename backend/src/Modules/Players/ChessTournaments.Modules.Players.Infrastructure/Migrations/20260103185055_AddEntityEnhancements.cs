using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Players.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Players",
                table: "Players",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Players",
                table: "Players",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Players",
                table: "Players",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Players",
                table: "Achievements",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Players",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Players",
                table: "Achievements",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DeletedAt", schema: "Players", table: "Players");

            migrationBuilder.DropColumn(name: "IsDeleted", schema: "Players", table: "Players");

            migrationBuilder.DropColumn(name: "RowVersion", schema: "Players", table: "Players");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Players",
                table: "Achievements"
            );

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Players",
                table: "Achievements"
            );

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "Players",
                table: "Achievements"
            );
        }
    }
}
