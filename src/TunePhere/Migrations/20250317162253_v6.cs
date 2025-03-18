using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class v6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants",
                column: "RoomId",
                principalTable: "ListeningRooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants",
                column: "RoomId",
                principalTable: "ListeningRooms",
                principalColumn: "RoomId");
        }
    }
}
