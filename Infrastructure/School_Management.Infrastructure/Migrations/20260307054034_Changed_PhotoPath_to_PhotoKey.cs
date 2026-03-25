using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace School_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changed_PhotoPath_to_PhotoKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoPath",
                table: "Employees",
                newName: "PhotoKey");

            migrationBuilder.RenameColumn(
                name: "PhotoPath",
                table: "Candidates",
                newName: "PhotoKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoKey",
                table: "Employees",
                newName: "PhotoPath");

            migrationBuilder.RenameColumn(
                name: "PhotoKey",
                table: "Candidates",
                newName: "PhotoPath");
        }
    }
}
