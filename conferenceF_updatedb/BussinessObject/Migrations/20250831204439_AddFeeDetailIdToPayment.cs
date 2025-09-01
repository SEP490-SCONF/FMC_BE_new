using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeDetailIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FeeDetailId",
                table: "Payment",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_FeeDetailId",
                table: "Payment",
                column: "FeeDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_FeeDetails_FeeDetailId",
                table: "Payment",
                column: "FeeDetailId",
                principalTable: "FeeDetails",
                principalColumn: "FeeDetailId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_FeeDetails_FeeDetailId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_FeeDetailId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "FeeDetailId",
                table: "Payment");
        }

    }
}
