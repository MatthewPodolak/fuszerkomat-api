using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class locnameworktask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "WorkTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "WorkTasks");
        }
    }
}
