using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class Tune5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "ArtistFollowers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistFollowers_AppUserId",
                table: "ArtistFollowers",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistFollowers_AspNetUsers_AppUserId",
                table: "ArtistFollowers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistFollowers_AspNetUsers_AppUserId",
                table: "ArtistFollowers");

            migrationBuilder.DropIndex(
                name: "IX_ArtistFollowers_AppUserId",
                table: "ArtistFollowers");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "ArtistFollowers");
        }
    }
}
