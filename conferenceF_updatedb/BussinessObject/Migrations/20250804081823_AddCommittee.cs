using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddCommittee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Affiliation",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                table: "UserConferenceRole",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserConferenceRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DisplayNameOverride",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Expertise",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "UserConferenceRole",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "UserConferenceRole",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "UserConferenceRole",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecificTitle",
                table: "UserConferenceRole",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserConferenceRole",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Affiliation",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "DisplayNameOverride",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "Expertise",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "SpecificTitle",
                table: "UserConferenceRole");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserConferenceRole");
        }
    }
}
