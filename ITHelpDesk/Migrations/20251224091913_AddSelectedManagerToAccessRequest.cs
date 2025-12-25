using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddSelectedManagerToAccessRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedManagerId",
                table: "AccessRequests",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_SelectedManagerId",
                table: "AccessRequests",
                column: "SelectedManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRequests_AspNetUsers_SelectedManagerId",
                table: "AccessRequests",
                column: "SelectedManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessRequests_AspNetUsers_SelectedManagerId",
                table: "AccessRequests");

            migrationBuilder.DropIndex(
                name: "IX_AccessRequests_SelectedManagerId",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "SelectedManagerId",
                table: "AccessRequests");
        }
    }
}
