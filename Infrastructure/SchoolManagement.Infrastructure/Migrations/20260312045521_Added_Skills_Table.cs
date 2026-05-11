using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Skills_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skill",
                table: "Candidates");

            migrationBuilder.AddColumn<int>(
                name: "SkillId",
                table: "Candidates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidates_SkillId",
                table: "Candidates",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidates_Skills_SkillId",
                table: "Candidates",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidates_Skills_SkillId",
                table: "Candidates");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Candidates_SkillId",
                table: "Candidates");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "Candidates");

            migrationBuilder.AddColumn<string>(
                name: "Skill",
                table: "Candidates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
