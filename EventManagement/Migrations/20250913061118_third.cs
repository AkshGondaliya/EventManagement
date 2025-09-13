using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Migrations
{
    public partial class third : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CollegeName",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Events",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$UG9etNMcvDmMK47BqsDQl.DFxuCKrCQrfMc4VlvoLoefWhvwnAqSu");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CollegeName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$CpI1W9UaWPgJcO6iLuBXbuAD.HTQyT55M/FjN8DsI5ftmqQA0IM4C");
        }
    }
}
