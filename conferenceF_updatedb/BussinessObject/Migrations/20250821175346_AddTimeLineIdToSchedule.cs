using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLineIdToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeLineId",
                table: "Schedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_TimeLineId",
                table: "Schedule",
                column: "TimeLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedule_TimeLine",
                table: "Schedule",
                column: "TimeLineId",
                principalTable: "TimeLine",
                principalColumn: "TimeLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedule_TimeLine",
                table: "Schedule");

            migrationBuilder.DropIndex(
                name: "IX_Schedule_TimeLineId",
                table: "Schedule");

            migrationBuilder.DropColumn(
                name: "TimeLineId",
                table: "Schedule");
        }
    }
}
