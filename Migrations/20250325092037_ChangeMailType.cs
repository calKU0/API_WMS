using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIWMS.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMailType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Flow",
                schema: "kkur",
                table: "ApiLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MailSent",
                schema: "kkur",
                table: "ApiLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailSent",
                schema: "kkur",
                table: "ApiLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Flow",
                schema: "kkur",
                table: "ApiLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
