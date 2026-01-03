using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class Added_Enums_to_Gender_and_StayType_and_Added_SiblingsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "Students",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "SiblingsCount",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StayType",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SiblingsCount",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StayType",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Students",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
