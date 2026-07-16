using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addcomponentstosubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubjectComponents_Subjects_SubjectId",
                table: "SubjectComponents");

            migrationBuilder.DropIndex(
                name: "IX_SubjectComponents_SubjectId",
                table: "SubjectComponents");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "SubjectComponents");

            migrationBuilder.AddColumn<int>(
                name: "MapperId",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SubjectMappers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComponentId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectMappers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectMappers_SubjectComponents_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "SubjectComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectMappers_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectComponents_Name",
                table: "SubjectComponents",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scores_MapperId",
                table: "Scores",
                column: "MapperId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMappers_ComponentId",
                table: "SubjectMappers",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectMappers_SubjectId",
                table: "SubjectMappers",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_SubjectMappers_MapperId",
                table: "Scores",
                column: "MapperId",
                principalTable: "SubjectMappers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_SubjectMappers_MapperId",
                table: "Scores");

            migrationBuilder.DropTable(
                name: "SubjectMappers");

            migrationBuilder.DropIndex(
                name: "IX_SubjectComponents_Name",
                table: "SubjectComponents");

            migrationBuilder.DropIndex(
                name: "IX_Scores_MapperId",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "MapperId",
                table: "Scores");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "SubjectComponents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SubjectComponents_SubjectId",
                table: "SubjectComponents",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubjectComponents_Subjects_SubjectId",
                table: "SubjectComponents",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
