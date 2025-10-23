using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class companylocext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Lattitude",
                table: "Addresses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longtitude",
                table: "Addresses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lattitude",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Longtitude",
                table: "Addresses");
        }
    }
}
