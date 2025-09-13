using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "FullName", "PasswordHash", "ProfilePictureUrl", "Role" },
                values: new object[] { 100, "admin@college.edu", "System Admin", "$2b$10$CpI1W9UaWPgJcO6iLuBXbuAD.HTQyT55M/FjN8DsI5ftmqQA0IM4C", null, "Admin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100);
        }
    }
}
