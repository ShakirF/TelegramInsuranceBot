using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserUserStateRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserStates_UserId",
                table: "UserStates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStates_Users_UserId",
                table: "UserStates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStates_Users_UserId",
                table: "UserStates");

            migrationBuilder.DropIndex(
                name: "IX_UserStates_UserId",
                table: "UserStates");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserStates");
        }
    }
}
