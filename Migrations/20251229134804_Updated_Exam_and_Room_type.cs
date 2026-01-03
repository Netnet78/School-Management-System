using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Exam_and_Room_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Students"
                ALTER COLUMN "ExamTable" TYPE integer
                USING NULLIF(regexp_replace("ExamTable", '[^0-9]', '', 'g'), '')::integer,
                ALTER COLUMN "ExamTable" DROP NOT NULL,
                ALTER COLUMN "ExamTable" SET DEFAULT NULL;
                """
                );
            migrationBuilder.Sql(
                """
                ALTER TABLE "Students"
                ALTER COLUMN "ExamRoom" TYPE integer
                USING NULLIF(regexp_replace("ExamRoom", '[^0-9]', '', 'g'), '')::integer,
                ALTER COLUMN "ExamRoom" DROP NOT NULL,
                ALTER COLUMN "ExamRoom" SET DEFAULT NULL;
                """
                );

            migrationBuilder.AlterColumn<int>(
                name: "ExamRoom",
                table: "Students",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExamTable",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExamRoom",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
