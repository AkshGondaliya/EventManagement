using Microsoft.EntityFrameworkCore.Migrations;

namespace EventManagement.Migrations
{
    public partial class AddPaymentMethodToRegistration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Registrations",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$LG1FhigfHtbDqgmrFMtgwuJh9MevvVrb5z1OXJ47lQl2xHTCFXyqG");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Registrations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 100,
                column: "PasswordHash",
                value: "$2b$10$0JygYc8yvwwA3CJ1LH69iuF7woaIGiYiTbfTzzAl3HX7O4TwqF/ky");
        }
    }
}
