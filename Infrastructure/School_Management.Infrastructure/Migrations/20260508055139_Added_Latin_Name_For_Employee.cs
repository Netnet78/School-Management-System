using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace School_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Latin_Name_For_Employee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LatinFullName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatinFullName",
                table: "Employees");
        }
    }
}
