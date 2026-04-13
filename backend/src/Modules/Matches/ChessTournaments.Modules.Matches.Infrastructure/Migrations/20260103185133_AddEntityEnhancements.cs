using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.Matches.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Matches",
                table: "MatchTags",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Matches",
                table: "MatchTags",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Matches",
                table: "MatchTags",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Matches",
                table: "Matches",
                type: "datetime2",
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Matches",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "Matches",
                table: "Matches",
                type: "rowversion",
                rowVersion: true,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DeletedAt", schema: "Matches", table: "MatchTags");

            migrationBuilder.DropColumn(name: "IsDeleted", schema: "Matches", table: "MatchTags");

            migrationBuilder.DropColumn(name: "RowVersion", schema: "Matches", table: "MatchTags");

            migrationBuilder.DropColumn(name: "DeletedAt", schema: "Matches", table: "Matches");

            migrationBuilder.DropColumn(name: "IsDeleted", schema: "Matches", table: "Matches");

            migrationBuilder.DropColumn(name: "RowVersion", schema: "Matches", table: "Matches");
        }
    }
}
