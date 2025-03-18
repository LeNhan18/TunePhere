using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_Users_UserId",
                table: "ListeningRoomParticipants");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants",
                column: "RoomId",
                principalTable: "ListeningRooms",
                principalColumn: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_Users_UserId",
                table: "ListeningRoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_Users_UserId",
                table: "ListeningRoomParticipants");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants",
                column: "RoomId",
                principalTable: "ListeningRooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_Users_UserId",
                table: "ListeningRoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
