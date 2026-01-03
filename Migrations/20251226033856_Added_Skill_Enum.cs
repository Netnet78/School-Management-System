using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class Added_Skill_Enum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Students" SET "Skill" =
                    CASE "Skill"
                        WHEN 'Computer' THEN 0
                        WHEN 'Electrical' THEN 1
                        WHEN 'CNC' THEN 2
                        WHEN 'General' THEN 3
                        ELSE 0
                    END;

                ALTER TABLE "Students"
                    ALTER COLUMN "Skill" TYPE integer USING "Skill"::integer;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Skill",
                table: "Students",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
