using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adventure19.Migrations
{
    /// <inheritdoc />
    public partial class updateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Full_Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__3214EC07A4E5AE01", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__536C85E412C322C4",
                table: "Customer",
                column: "Full_Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Customer__A9D10534D5B65573",
                table: "Customer",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
