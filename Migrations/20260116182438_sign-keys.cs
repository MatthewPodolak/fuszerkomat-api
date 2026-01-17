using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class signkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivateSignKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicSignKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateSignKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PublicSignKey",
                table: "AspNetUsers");
        }
    }
}
