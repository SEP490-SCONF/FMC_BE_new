using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BussinessObject.Migrations
{
    /// <inheritdoc />
    public partial class AddHighlightArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HighlightArea",
                columns: table => new
                {
                    HighlightAreaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HighlightId = table.Column<int>(type: "int", nullable: false),
                    PageIndex = table.Column<int>(type: "int", nullable: true),
                    Left = table.Column<double>(type: "float", nullable: true),
                    Top = table.Column<double>(type: "float", nullable: true),
                    Width = table.Column<double>(type: "float", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    TextHighlighted = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighlightArea", x => x.HighlightAreaId);
                    table.ForeignKey(
                        name: "FK_HighlightArea_ReviewHighlight_HighlightId",
                        column: x => x.HighlightId,
                        principalTable: "ReviewHighlight",
                        principalColumn: "HighlightId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HighlightArea_HighlightId",
                table: "HighlightArea",
                column: "HighlightId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HighlightArea");
        }
    }
}
