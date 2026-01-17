using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fuszerkomat_api.Migrations
{
    /// <inheritdoc />
    public partial class newmototag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 11, 15 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 103,
                column: "TagType",
                value: 102);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 104,
                column: "TagType",
                value: 103);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 105,
                column: "TagType",
                value: 104);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 106,
                column: "TagType",
                value: 105);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 107,
                column: "TagType",
                value: 106);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 12, 107 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 109,
                column: "TagType",
                value: 108);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 110,
                column: "TagType",
                value: 109);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 111,
                column: "TagType",
                value: 110);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 112,
                column: "TagType",
                value: 111);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 113,
                column: "TagType",
                value: 112);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 114,
                column: "TagType",
                value: 113);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 13, 114 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 116,
                column: "TagType",
                value: 115);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 117,
                column: "TagType",
                value: 116);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 118,
                column: "TagType",
                value: 117);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 119,
                column: "TagType",
                value: 118);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 120,
                column: "TagType",
                value: 119);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 121,
                column: "TagType",
                value: 120);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 14, 121 });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "CategoryType", "TagType" },
                values: new object[] { 123, 15, 122 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 123);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 12, 102 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 103,
                column: "TagType",
                value: 103);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 104,
                column: "TagType",
                value: 104);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 105,
                column: "TagType",
                value: 105);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 106,
                column: "TagType",
                value: 106);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 107,
                column: "TagType",
                value: 107);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 108,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 13, 108 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 109,
                column: "TagType",
                value: 109);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 110,
                column: "TagType",
                value: 110);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 111,
                column: "TagType",
                value: 111);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 112,
                column: "TagType",
                value: 112);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 113,
                column: "TagType",
                value: 113);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 114,
                column: "TagType",
                value: 114);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 115,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 14, 115 });

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 116,
                column: "TagType",
                value: 116);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 117,
                column: "TagType",
                value: 117);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 118,
                column: "TagType",
                value: 118);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 119,
                column: "TagType",
                value: 119);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 120,
                column: "TagType",
                value: 120);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 121,
                column: "TagType",
                value: 121);

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 122,
                columns: new[] { "CategoryType", "TagType" },
                values: new object[] { 15, 122 });
        }
    }
}
