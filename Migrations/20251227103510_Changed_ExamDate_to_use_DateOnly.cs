using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class Changed_ExamDate_to_use_DateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Students" ALTER COLUMN "ExamDate" TYPE date USING TO_DATE("ExamDate", 'DD/MM/YYYY');
                """
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ExamDate",
                table: "Students",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
