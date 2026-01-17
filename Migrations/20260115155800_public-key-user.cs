using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class publickeyuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicKey",
                table: "AspNetUsers");
        }
    }
}
