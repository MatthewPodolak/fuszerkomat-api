using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class worktaskgallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "WorkTasks",
                newName: "MaxPrice");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "WorkTasks",
                newName: "ExpectedRealisationTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "WorkTasks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "WorkTaskGalleries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkTaskId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTaskGalleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTaskGalleries_WorkTasks_WorkTaskId",
                        column: x => x.WorkTaskId,
                        principalTable: "WorkTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTaskGalleries_WorkTaskId",
                table: "WorkTaskGalleries",
                column: "WorkTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkTaskGalleries");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "WorkTasks");

            migrationBuilder.RenameColumn(
                name: "MaxPrice",
                table: "WorkTasks",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "ExpectedRealisationTime",
                table: "WorkTasks",
                newName: "Currency");

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "WorkTasks",
                type: "datetime2",
                nullable: true);
        }
    }
}
