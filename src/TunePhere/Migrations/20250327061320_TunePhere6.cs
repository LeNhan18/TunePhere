using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class TunePhere6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Songs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Songs");
        }
    }
}
