using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Migrations
{
    public partial class RefactorModelsForNewFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "Registrations",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Semester",
                table: "Registrations",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$8CdJjrhV2QbPmEIMFKs8b.jbN6dmW74LYNR3kKngoUCLXExJ5jm32");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "Registrations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$kpZgnegpH.aREMA0YJOYC.GpYtzueUQNw.P0EhR0kTbpS/GIsRYWq");
        }
    }
}
