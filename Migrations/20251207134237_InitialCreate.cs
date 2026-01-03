using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace New_Student_Management.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Skill = table.Column<string>(type: "text", nullable: false),
                    BirthVillage = table.Column<string>(type: "text", nullable: false),
                    BirthCommune = table.Column<string>(type: "text", nullable: false),
                    BirthDistrict = table.Column<string>(type: "text", nullable: false),
                    BirthProvince = table.Column<string>(type: "text", nullable: false),
                    FatherName = table.Column<string>(type: "text", nullable: false),
                    MotherName = table.Column<string>(type: "text", nullable: false),
                    FatherOccupation = table.Column<string>(type: "text", nullable: false),
                    MotherOccupation = table.Column<string>(type: "text", nullable: false),
                    Religion = table.Column<string>(type: "text", nullable: false),
                    PhotoPath = table.Column<string>(type: "text", nullable: false),
                    ExamCenter = table.Column<string>(type: "text", nullable: false),
                    ExamDate = table.Column<string>(type: "text", nullable: false),
                    ExamTable = table.Column<string>(type: "text", nullable: false),
                    ExamRoom = table.Column<string>(type: "text", nullable: false),
                    FromSchool = table.Column<string>(type: "text", nullable: false),
                    OtherInfo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
