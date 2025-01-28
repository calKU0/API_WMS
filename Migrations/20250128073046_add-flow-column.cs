using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIWMS.Migrations
{
    /// <inheritdoc />
    public partial class addflowcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Flow",
                schema: "kkur",
                table: "ApiLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flow",
                schema: "kkur",
                table: "ApiLogs");
        }
    }
}
