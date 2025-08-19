using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationAndEndTimeToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Schedule__Confer__2BFE89A6",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK__Schedule__PaperI__2CF2ADDF",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK__Schedule__Presen__2DE6D218",
                table: "Schedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Schedule__9C8A5B49A7037950",
                table: "Schedule");

            migrationBuilder.RenameColumn(
                name: "PresentationTime",
                table: "Schedule",
                newName: "PresentationStartTime");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Schedule",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PresentationEndTime",
                table: "Schedule",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Conference_ConferenceId",
                table: "Schedule",
                column: "ConferenceId",
                principalTable: "Conference",
                principalColumn: "ConferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_Paper_PaperId",
                table: "Schedule",
                column: "PaperId",
                principalTable: "Paper",
                principalColumn: "PaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_User_PresenterId",
                table: "Schedule",
                column: "PresenterId",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Conference_ConferenceId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_Paper_PaperId",
                table: "Schedule");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_User_PresenterId",
                table: "Schedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Schedule",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "PresentationEndTime",
                table: "Schedule");

            migrationBuilder.RenameColumn(
                name: "PresentationStartTime",
                table: "Schedule",
                newName: "PresentationTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Schedule__9C8A5B49A7037950",
                table: "Schedule",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK__Schedule__Confer__2BFE89A6",
                table: "Schedule",
                column: "ConferenceId",
                principalTable: "Conference",
                principalColumn: "ConferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK__Schedule__PaperI__2CF2ADDF",
                table: "Schedule",
                column: "PaperId",
                principalTable: "Paper",
                principalColumn: "PaperId");

            migrationBuilder.AddForeignKey(
                name: "FK__Schedule__Presen__2DE6D218",
                table: "Schedule",
                column: "PresenterId",
                principalTable: "User",
                principalColumn: "UserId");
        }
    }
}
