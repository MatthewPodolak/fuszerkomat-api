using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class opinionworktask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkTaskId",
                table: "Opinions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_WorkTaskId",
                table: "Opinions",
                column: "WorkTaskId",
                unique: true,
                filter: "[WorkTaskId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Opinions_WorkTasks_WorkTaskId",
                table: "Opinions",
                column: "WorkTaskId",
                principalTable: "WorkTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Opinions_WorkTasks_WorkTaskId",
                table: "Opinions");

            migrationBuilder.DropIndex(
                name: "IX_Opinions_WorkTaskId",
                table: "Opinions");

            migrationBuilder.DropColumn(
                name: "WorkTaskId",
                table: "Opinions");
        }
    }
}
