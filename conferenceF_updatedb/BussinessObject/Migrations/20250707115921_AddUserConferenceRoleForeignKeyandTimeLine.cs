using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserConferenceRoleForeignKeyandTimeLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserConferenceRoleId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserConferenceRoleId",
                table: "Certificate",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimeLine",
                columns: table => new
                {
                    TimeLineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConferenceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeLine", x => x.TimeLineId);
                    table.ForeignKey(
                        name: "FK_TimeLine_Conference_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conference",
                        principalColumn: "ConferenceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserConferenceRoleId",
                table: "Notifications",
                column: "UserConferenceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_UserConferenceRoleId",
                table: "Certificate",
                column: "UserConferenceRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLine_ConferenceId",
                table: "TimeLine",
                column: "ConferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificate_UserConferenceRole_UserConferenceRoleId",
                table: "Certificate",
                column: "UserConferenceRoleId",
                principalTable: "UserConferenceRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_UserConferenceRole_UserConferenceRoleId",
                table: "Notifications",
                column: "UserConferenceRoleId",
                principalTable: "UserConferenceRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificate_UserConferenceRole_UserConferenceRoleId",
                table: "Certificate");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_UserConferenceRole_UserConferenceRoleId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "TimeLine");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserConferenceRoleId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Certificate_UserConferenceRoleId",
                table: "Certificate");

            migrationBuilder.DropColumn(
                name: "UserConferenceRoleId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserConferenceRoleId",
                table: "Certificate");
        }
    }
}
