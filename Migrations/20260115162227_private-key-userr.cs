using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class privatekeyuserr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivateKey",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKey",
                table: "AspNetUsers");
        }
    }
}
