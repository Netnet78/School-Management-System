using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Made_StudentQR_One_To_One : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQRs_Students_StudentId",
                table: "StudentQRs");

            migrationBuilder.DropIndex(
                name: "IX_StudentQRs_StudentId",
                table: "StudentQRs");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "StudentQRs");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StudentQRs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQRs_Students_Id",
                table: "StudentQRs",
                column: "Id",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentQRs_Students_Id",
                table: "StudentQRs");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StudentQRs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "StudentQRs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentQRs_StudentId",
                table: "StudentQRs",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentQRs_Students_StudentId",
                table: "StudentQRs",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
