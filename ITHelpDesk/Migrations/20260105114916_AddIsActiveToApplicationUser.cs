using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_HardDisks_VirtualMachine_HardDiskId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Keyboards_VirtualMachine_KeyboardId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Mice_VirtualMachine_MouseId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Monitors_VirtualMachine_MonitorId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Processors_VirtualMachine_ProcessorId",
                table: "Assets");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "SystemChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    RequesterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChangePriority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChangeImpact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AffectedAssets = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImplementationPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    BackoutPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ImplementerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManagerApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    ManagerApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SecurityApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    SecurityApprovalComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SelectedManagerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemChangeRequests_AspNetUsers_SelectedManagerId",
                        column: x => x.SelectedManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemChangeRequests_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemChangeRequests_SelectedManagerId",
                table: "SystemChangeRequests",
                column: "SelectedManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemChangeRequests_TicketId",
                table: "SystemChangeRequests",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId",
                table: "Assets",
                column: "VirtualMachine_ComputerInfoId",
                principalTable: "ComputerInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_HardDisks_VirtualMachine_HardDiskId",
                table: "Assets",
                column: "VirtualMachine_HardDiskId",
                principalTable: "HardDisks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Keyboards_VirtualMachine_KeyboardId",
                table: "Assets",
                column: "VirtualMachine_KeyboardId",
                principalTable: "Keyboards",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId",
                table: "Assets",
                column: "VirtualMachine_MemoryDetailsId",
                principalTable: "MemoryDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Mice_VirtualMachine_MouseId",
                table: "Assets",
                column: "VirtualMachine_MouseId",
                principalTable: "Mice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Monitors_VirtualMachine_MonitorId",
                table: "Assets",
                column: "VirtualMachine_MonitorId",
                principalTable: "Monitors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId",
                table: "Assets",
                column: "VirtualMachine_OperatingSystemInfoId",
                principalTable: "OperatingSystemInfos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Processors_VirtualMachine_ProcessorId",
                table: "Assets",
                column: "VirtualMachine_ProcessorId",
                principalTable: "Processors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_HardDisks_VirtualMachine_HardDiskId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Keyboards_VirtualMachine_KeyboardId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Mice_VirtualMachine_MouseId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Monitors_VirtualMachine_MonitorId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Processors_VirtualMachine_ProcessorId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "SystemChangeRequests");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId",
                table: "Assets",
                column: "VirtualMachine_ComputerInfoId",
                principalTable: "ComputerInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_HardDisks_VirtualMachine_HardDiskId",
                table: "Assets",
                column: "VirtualMachine_HardDiskId",
                principalTable: "HardDisks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Keyboards_VirtualMachine_KeyboardId",
                table: "Assets",
                column: "VirtualMachine_KeyboardId",
                principalTable: "Keyboards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId",
                table: "Assets",
                column: "VirtualMachine_MemoryDetailsId",
                principalTable: "MemoryDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Mice_VirtualMachine_MouseId",
                table: "Assets",
                column: "VirtualMachine_MouseId",
                principalTable: "Mice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Monitors_VirtualMachine_MonitorId",
                table: "Assets",
                column: "VirtualMachine_MonitorId",
                principalTable: "Monitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId",
                table: "Assets",
                column: "VirtualMachine_OperatingSystemInfoId",
                principalTable: "OperatingSystemInfos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Processors_VirtualMachine_ProcessorId",
                table: "Assets",
                column: "VirtualMachine_ProcessorId",
                principalTable: "Processors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
