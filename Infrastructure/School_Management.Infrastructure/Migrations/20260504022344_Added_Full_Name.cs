using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace School_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Full_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Candidates",
                type: "text",
                nullable: false,
                computedColumnSql: "\"LastName\" || ' ' || \"FirstName\"",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "LatinFullName",
                table: "Candidates",
                type: "text",
                nullable: false,
                computedColumnSql: "\"LatinLastName\" || ' ' || \"LatinFirstName\"",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "LatinFullName",
                table: "Candidates");
        }
    }
}
