using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class Added_Latin_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LatinFirstName",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LatinLastName",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatinFirstName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LatinLastName",
                table: "Students");
        }
    }
}
