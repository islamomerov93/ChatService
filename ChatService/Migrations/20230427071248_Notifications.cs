using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatService.Migrations
{
    /// <inheritdoc />
    public partial class Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ChatUsers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subjet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChatId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationUsers",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false),
                    ChatUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationUsers", x => new { x.NotificationId, x.ChatUserId });
                    table.ForeignKey(
                        name: "FK_NotificationUsers_ChatUsers_ChatUserId",
                        column: x => x.ChatUserId,
                        principalTable: "ChatUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationUsers_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_ChatId",
                table: "ChatUsers",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ChatId",
                table: "Notifications",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationUsers_ChatUserId",
                table: "NotificationUsers",
                column: "ChatUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationUsers");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers");

            migrationBuilder.DropIndex(
                name: "IX_ChatUsers_ChatId",
                table: "ChatUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatUsers",
                table: "ChatUsers",
                columns: new[] { "ChatId", "UserId" });
        }
    }
}
