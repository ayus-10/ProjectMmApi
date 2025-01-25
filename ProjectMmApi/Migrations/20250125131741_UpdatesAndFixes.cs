using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectMmApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatesAndFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageTime",
                table: "Conversations");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Messages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MessageTime",
                table: "Messages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageTime",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Conversations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastMessageId",
                table: "Conversations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageTime",
                table: "Conversations",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
