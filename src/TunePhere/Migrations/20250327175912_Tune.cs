using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class Tune : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollowers_AspNetUsers_FollowedId",
                table: "UserFollowers");

            migrationBuilder.RenameColumn(
                name: "FollowedId",
                table: "UserFollowers",
                newName: "FollowingId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "UserFollowers",
                newName: "FollowedAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollowers_FollowedId",
                table: "UserFollowers",
                newName: "IX_UserFollowers_FollowingId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollowers_AspNetUsers_FollowingId",
                table: "UserFollowers",
                column: "FollowingId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserFollowers_AspNetUsers_FollowingId",
                table: "UserFollowers");

            migrationBuilder.RenameColumn(
                name: "FollowingId",
                table: "UserFollowers",
                newName: "FollowedId");

            migrationBuilder.RenameColumn(
                name: "FollowedAt",
                table: "UserFollowers",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserFollowers_FollowingId",
                table: "UserFollowers",
                newName: "IX_UserFollowers_FollowedId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFollowers_AspNetUsers_FollowedId",
                table: "UserFollowers",
                column: "FollowedId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
