using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class SeedFeeType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "FeeTypes",
                columns: new[] { "FeeTypeId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Phí đăng ký tác giả / Author registration fee", "Registration" },
                    { 2, "Phí tham dự hội thảo (listener) / Conference participation fee (listener)", "Participation" },
                    { 3, "Phí vượt số trang in ấn / Additional page fee", "Additional Page" },
                    { 4, "Phí mua tài liệu/kỷ yếu / Proceedings access fee", "Proceedings Access" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FeeTypes",
                keyColumn: "FeeTypeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FeeTypes",
                keyColumn: "FeeTypeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FeeTypes",
                keyColumn: "FeeTypeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FeeTypes",
                keyColumn: "FeeTypeId",
                keyValue: 4);
        }
    }
}
