using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperStatusToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Review__Revision__245D67DE",
                table: "Review");

            migrationBuilder.AlterColumn<int>(
                name: "RevisionId",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaperId",
                table: "Review",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "PaperStatus",
                table: "Review",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__Review__Revision__245D67DE",
                table: "Review",
                column: "RevisionId",
                principalTable: "PaperRevision",
                principalColumn: "RevisionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Review__Revision__245D67DE",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "PaperStatus",
                table: "Review");

            migrationBuilder.AlterColumn<int>(
                name: "RevisionId",
                table: "Review",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PaperId",
                table: "Review",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK__Review__Revision__245D67DE",
                table: "Review",
                column: "RevisionId",
                principalTable: "PaperRevision",
                principalColumn: "RevisionId");
        }
    }
}
