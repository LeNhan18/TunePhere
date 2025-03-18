using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunePhere.Migrations
{
    /// <inheritdoc />
    public partial class TunePhere1 : Migration
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

            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRooms_Users_HostUserId",
                table: "ListeningRooms");

            migrationBuilder.RenameColumn(
                name: "HostUserId",
                table: "ListeningRooms",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_ListeningRooms_HostUserId",
                table: "ListeningRooms",
                newName: "IX_ListeningRooms_CreatorId");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Lyrics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Lyrics",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRooms_Users_CreatorId",
                table: "ListeningRooms",
                column: "CreatorId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_ListeningRooms_Users_CreatorId",
                table: "ListeningRooms");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Lyrics");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "ListeningRooms",
                newName: "HostUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ListeningRooms_CreatorId",
                table: "ListeningRooms",
                newName: "IX_ListeningRooms_HostUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Lyrics",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_ListeningRooms_RoomId",
                table: "ListeningRoomParticipants",
                column: "RoomId",
                principalTable: "ListeningRooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRoomParticipants_Users_UserId",
                table: "ListeningRoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ListeningRooms_Users_HostUserId",
                table: "ListeningRooms",
                column: "HostUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
