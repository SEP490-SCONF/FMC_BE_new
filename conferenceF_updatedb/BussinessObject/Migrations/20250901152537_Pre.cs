using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class Pre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "FeeTypes",
                columns: new[] { "FeeTypeId", "Description", "Name" },
                values: new object[] { 5, "Phí trình bày bài báo/ Presentation fee", "Presentation" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FeeTypes",
                keyColumn: "FeeTypeId",
                keyValue: 5);
        }
    }
}
