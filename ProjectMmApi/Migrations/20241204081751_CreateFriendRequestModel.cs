using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMmApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateFriendRequestModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FriendRequests",
                columns: table => new
                {
                    FriendRequestId = table.Column<Guid>(type: "char(36)", nullable: false),
                    SenderId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequests", x => x.FriendRequestId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendRequests");
        }
    }
}
