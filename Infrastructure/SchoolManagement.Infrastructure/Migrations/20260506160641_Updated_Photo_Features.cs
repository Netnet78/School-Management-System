using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Updated_Photo_Features : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: true),
                    LocalPath = table.Column<string>(type: "text", nullable: true),
                    FileStatus = table.Column<int>(type: "integer", nullable: false),
                    LastAttempt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPhotos_Candidates_Id",
                        column: x => x.Id,
                        principalTable: "Candidates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""StudentPhotos"" (""Id"", ""Key"", ""FileStatus"")
                SELECT ""Id"", ""PhotoKey"", 1
                FROM ""Candidates""
                WHERE NULLIF(""PhotoKey"", '') IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "PhotoKey",
                table: "Candidates");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentPhotos");

            migrationBuilder.AddColumn<string>(
                name: "PhotoKey",
                table: "Candidates",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
