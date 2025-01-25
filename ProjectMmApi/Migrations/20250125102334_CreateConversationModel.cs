using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMmApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateConversationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConversationId",
                table: "Friends",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FriendId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LastMessageId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LastMessageTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsSeen = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_Conversations_Friends_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Friends",
                        principalColumn: "FriendId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FriendId",
                table: "Conversations",
                column: "FriendId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Friends");
        }
    }
}
