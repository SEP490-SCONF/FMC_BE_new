using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class seedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConferenceRole",
                columns: new[] { "ConferenceRoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Participate" },
                    { 2, "Author" },
                    { 3, "Reviewer" },
                    { 4, "Organizer" }
                });

            migrationBuilder.InsertData(
                table: "Topic",
                columns: new[] { "TopicId", "Status", "TopicName" },
                values: new object[,]
                {
                    { 1, true, "Machine Learning" },
                    { 2, true, "Deep Learning" },
                    { 3, true, "Natural Language Processing" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "AvatarUrl", "CreatedAt", "Email", "Name", "RefreshToken", "RoleId", "Status", "TokenExpiry" },
                values: new object[,]
                {
                    { 1, "https://conferencefmc.blob.core.windows.net/avatars/83869135-4125-4b69-9956-7a20b7b5dddd.webp", new DateTime(2025, 7, 24, 3, 8, 59, 710, DateTimeKind.Unspecified), "kienkoi48@gmail.com", "Tín Trương Văn", "fd82b31874b148c9aeed3f495864272f", 1, true, new DateTime(2025, 8, 30, 4, 3, 12, 323, DateTimeKind.Unspecified) },
                    { 4, "https://lh3.googleusercontent.com/a/ACg8ocIPgmy1R26U8Ralo0ZWyW6OtYZNrjeRl_x-SEf2L-05A9sbSVu2=s96-c", new DateTime(2025, 8, 2, 2, 38, 15, 200, DateTimeKind.Unspecified), "ffffffjj7@gmail.com", "Anh Nguyễn", "e4a467b7aff94bdf8cd3cf7ae798adc2", 1, true, new DateTime(2025, 8, 30, 8, 34, 4, 903, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConferenceRole",
                keyColumn: "ConferenceRoleId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ConferenceRole",
                keyColumn: "ConferenceRoleId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ConferenceRole",
                keyColumn: "ConferenceRoleId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ConferenceRole",
                keyColumn: "ConferenceRoleId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "TopicId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "TopicId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Topic",
                keyColumn: "TopicId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "UserId",
                keyValue: 4);
        }
    }
}
