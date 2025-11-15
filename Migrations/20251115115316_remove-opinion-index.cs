using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class removeopinionindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Opinions_AuthorUserId_CompanyId",
                table: "Opinions");

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_AuthorUserId",
                table: "Opinions",
                column: "AuthorUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Opinions_AuthorUserId",
                table: "Opinions");

            migrationBuilder.CreateIndex(
                name: "IX_Opinions_AuthorUserId_CompanyId",
                table: "Opinions",
                columns: new[] { "AuthorUserId", "CompanyId" },
                unique: true);
        }
    }
}
