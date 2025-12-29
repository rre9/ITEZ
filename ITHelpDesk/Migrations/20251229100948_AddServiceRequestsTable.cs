using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop table if it exists (in case it was created incorrectly)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceRequests]') AND type in (N'U'))
                BEGIN
                    DROP TABLE [dbo].[ServiceRequests];
                END
            ");

            migrationBuilder.CreateTable(
                name: "ServiceRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsageDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsageReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Acknowledged = table.Column<bool>(type: "bit", nullable: false),
                    SignatureName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SignatureJobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SignatureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelectedManagerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ManagerApprovalName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ManagerApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ITApprovalName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ITApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ITApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SecurityApprovalName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SecurityApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_AspNetUsers_SelectedManagerId",
                        column: x => x.SelectedManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceRequests_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SelectedManagerId",
                table: "ServiceRequests",
                column: "SelectedManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_TicketId",
                table: "ServiceRequests",
                column: "TicketId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceRequests");
        }
    }
}
