using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessTournaments.Modules.TournamentRequests.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTournamentRequestToParticipationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TournamentRequests_RequestedStartDate",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "AllowByes",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "CreatedTournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "EntryFee",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "Format",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "IncrementInSeconds",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "MaxPlayers",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "MinPlayers",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "NumberOfRounds",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "RequestedStartDate",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "TimeControl",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "TimeInMinutes",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "TournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_TournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "TournamentId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TournamentRequests_TournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.DropColumn(
                name: "TournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests"
            );

            migrationBuilder.AddColumn<bool>(
                name: "AllowByes",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "bit",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedTournamentId",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<decimal>(
                name: "EntryFee",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<string>(
                name: "Format",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "IncrementInSeconds",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "MaxPlayers",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "MinPlayers",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRounds",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedStartDate",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            );

            migrationBuilder.AddColumn<string>(
                name: "TimeControl",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<int>(
                name: "TimeInMinutes",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRequests_RequestedStartDate",
                schema: "TournamentRequests",
                table: "TournamentRequests",
                column: "RequestedStartDate"
            );
        }
    }
}
