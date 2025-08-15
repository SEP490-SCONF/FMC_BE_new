using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReviewHighlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OffsetEnd",
                table: "ReviewHighlight");

            migrationBuilder.DropColumn(
                name: "OffsetStart",
                table: "ReviewHighlight");

            migrationBuilder.RenameColumn(
                name: "PageNumber",
                table: "ReviewHighlight",
                newName: "PageIndex");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "ReviewHighlight",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Left",
                table: "ReviewHighlight",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Top",
                table: "ReviewHighlight",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "ReviewHighlight",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaperId",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "ReviewHighlight");

            migrationBuilder.DropColumn(
                name: "Left",
                table: "ReviewHighlight");

            migrationBuilder.DropColumn(
                name: "Top",
                table: "ReviewHighlight");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ReviewHighlight");

            migrationBuilder.RenameColumn(
                name: "PageIndex",
                table: "ReviewHighlight",
                newName: "PageNumber");

            migrationBuilder.AddColumn<int>(
                name: "OffsetEnd",
                table: "ReviewHighlight",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OffsetStart",
                table: "ReviewHighlight",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaperId",
                table: "Review",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
