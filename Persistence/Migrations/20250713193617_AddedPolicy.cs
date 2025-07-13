using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "ExtractedFields");

            migrationBuilder.AddColumn<int>(
                name: "CancelRetryCount",
                table: "UserStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelRetryCount",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Documents");

            migrationBuilder.AddColumn<float>(
                name: "Confidence",
                table: "ExtractedFields",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
