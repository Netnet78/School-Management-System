using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Advanced_features_for_subjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_ClassSubjects_ClassSubjectId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Exams_ExamId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_StudentClasses_StudentClassId",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_Scores_ClassSubjectId",
                table: "Scores");

            migrationBuilder.RenameColumn(
                name: "StudentClassId",
                table: "Scores",
                newName: "ComponentId");

            migrationBuilder.RenameColumn(
                name: "ExamId",
                table: "Scores",
                newName: "AssessmentId");

            migrationBuilder.RenameColumn(
                name: "ClassSubjectId",
                table: "Scores",
                newName: "AssessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_StudentClassId",
                table: "Scores",
                newName: "IX_Scores_ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_ExamId",
                table: "Scores",
                newName: "IX_Scores_AssessmentId");

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TotalScore = table.Column<decimal>(type: "numeric", nullable: false),
                    ExamId = table.Column<int>(type: "integer", nullable: false),
                    StudentClassId = table.Column<int>(type: "integer", nullable: false),
                    ClassSubjectId = table.Column<int>(type: "integer", nullable: false),
                    OtherInfo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_ClassSubjects_ClassSubjectId",
                        column: x => x.ClassSubjectId,
                        principalTable: "ClassSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Exams_ExamId",
                        column: x => x.ExamId,
                        principalTable: "Exams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_StudentClasses_StudentClassId",
                        column: x => x.StudentClassId,
                        principalTable: "StudentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubjectComponents_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ClassSubjectId",
                table: "Assessments",
                column: "ClassSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ExamId",
                table: "Assessments",
                column: "ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentClassId",
                table: "Assessments",
                column: "StudentClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectComponents_SubjectId",
                table: "SubjectComponents",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Assessments_AssessmentId",
                table: "Scores",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_SubjectComponents_ComponentId",
                table: "Scores",
                column: "ComponentId",
                principalTable: "SubjectComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Assessments_AssessmentId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_SubjectComponents_ComponentId",
                table: "Scores");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "SubjectComponents");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "Scores",
                newName: "StudentClassId");

            migrationBuilder.RenameColumn(
                name: "AssessmentId",
                table: "Scores",
                newName: "ExamId");

            migrationBuilder.RenameColumn(
                name: "AssessmentId",
                table: "Scores",
                newName: "ClassSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_ComponentId",
                table: "Scores",
                newName: "IX_Scores_StudentClassId");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_AssessmentId",
                table: "Scores",
                newName: "IX_Scores_ExamId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_ClassSubjectId",
                table: "Scores",
                column: "ClassSubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_ClassSubjects_ClassSubjectId",
                table: "Scores",
                column: "ClassSubjectId",
                principalTable: "ClassSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Exams_ExamId",
                table: "Scores",
                column: "ExamId",
                principalTable: "Exams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_StudentClasses_StudentClassId",
                table: "Scores",
                column: "StudentClassId",
                principalTable: "StudentClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
