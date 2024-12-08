using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMmApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationBetweenFriendAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Friends_ReceiverId",
                table: "Friends",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_SenderId",
                table: "Friends",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_ReceiverId",
                table: "Friends",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_Users_SenderId",
                table: "Friends",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_ReceiverId",
                table: "Friends");

            migrationBuilder.DropForeignKey(
                name: "FK_Friends_Users_SenderId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_ReceiverId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_SenderId",
                table: "Friends");
        }
    }
}
