using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpDesk.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssociatedTo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Site = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StateComments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComputerInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BiosDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SMBiosVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BiosVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BiosManufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputerInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HardDisks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CapacityGB = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HardDisks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Keyboards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyboardType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keyboards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemoryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RAM = table.Column<int>(type: "int", nullable: true),
                    VirtualMemory = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoryDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MouseType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MouseButtons = table.Column<int>(type: "int", nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IMEI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModelNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalCapacityGB = table.Column<int>(type: "int", nullable: true),
                    AvailableCapacityGB = table.Column<int>(type: "int", nullable: true),
                    ModemFirmwareVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Monitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxResolution = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NetworkDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IPAddress = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    MACAddress = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: true),
                    NIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Network = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultGateway = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DHCPEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DHCPServer = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DNSHostname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperatingSystemInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BuildNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ServicePack = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatingSystemInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Processors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessorInfo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClockSpeedMHz = table.Column<int>(type: "int", nullable: true),
                    NumberOfCores = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DoorNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Landmark = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StateProvince = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssetTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VendorId = table.Column<int>(type: "int", nullable: true),
                    PurchaseCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WarrantyExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssetStateId = table.Column<int>(type: "int", nullable: true),
                    NetworkDetailsId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssetType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    ComputerInfoId = table.Column<int>(type: "int", nullable: true),
                    OperatingSystemInfoId = table.Column<int>(type: "int", nullable: true),
                    MemoryDetailsId = table.Column<int>(type: "int", nullable: true),
                    ProcessorId = table.Column<int>(type: "int", nullable: true),
                    HardDiskId = table.Column<int>(type: "int", nullable: true),
                    KeyboardId = table.Column<int>(type: "int", nullable: true),
                    MouseId = table.Column<int>(type: "int", nullable: true),
                    MonitorId = table.Column<int>(type: "int", nullable: true),
                    MobileDetailsId = table.Column<int>(type: "int", nullable: true),
                    VMPlatform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VirtualHostId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_ComputerInfoId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_OperatingSystemInfoId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_MemoryDetailsId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_ProcessorId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_HardDiskId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_KeyboardId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_MouseId = table.Column<int>(type: "int", nullable: true),
                    VirtualMachine_MonitorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_AssetStates_AssetStateId",
                        column: x => x.AssetStateId,
                        principalTable: "AssetStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Assets_VirtualHostId",
                        column: x => x.VirtualHostId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_ComputerInfos_ComputerInfoId",
                        column: x => x.ComputerInfoId,
                        principalTable: "ComputerInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId",
                        column: x => x.VirtualMachine_ComputerInfoId,
                        principalTable: "ComputerInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_HardDisks_HardDiskId",
                        column: x => x.HardDiskId,
                        principalTable: "HardDisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_HardDisks_VirtualMachine_HardDiskId",
                        column: x => x.VirtualMachine_HardDiskId,
                        principalTable: "HardDisks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_Keyboards_KeyboardId",
                        column: x => x.KeyboardId,
                        principalTable: "Keyboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Keyboards_VirtualMachine_KeyboardId",
                        column: x => x.VirtualMachine_KeyboardId,
                        principalTable: "Keyboards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_MemoryDetails_MemoryDetailsId",
                        column: x => x.MemoryDetailsId,
                        principalTable: "MemoryDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId",
                        column: x => x.VirtualMachine_MemoryDetailsId,
                        principalTable: "MemoryDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_Mice_MouseId",
                        column: x => x.MouseId,
                        principalTable: "Mice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Mice_VirtualMachine_MouseId",
                        column: x => x.VirtualMachine_MouseId,
                        principalTable: "Mice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_MobileDetails_MobileDetailsId",
                        column: x => x.MobileDetailsId,
                        principalTable: "MobileDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Monitors_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Monitors_VirtualMachine_MonitorId",
                        column: x => x.VirtualMachine_MonitorId,
                        principalTable: "Monitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_NetworkDetails_NetworkDetailsId",
                        column: x => x.NetworkDetailsId,
                        principalTable: "NetworkDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_OperatingSystemInfos_OperatingSystemInfoId",
                        column: x => x.OperatingSystemInfoId,
                        principalTable: "OperatingSystemInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId",
                        column: x => x.VirtualMachine_OperatingSystemInfoId,
                        principalTable: "OperatingSystemInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_Processors_ProcessorId",
                        column: x => x.ProcessorId,
                        principalTable: "Processors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assets_Processors_VirtualMachine_ProcessorId",
                        column: x => x.VirtualMachine_ProcessorId,
                        principalTable: "Processors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assets_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetStateId",
                table: "Assets",
                column: "AssetStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ComputerInfoId",
                table: "Assets",
                column: "ComputerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CreatedById",
                table: "Assets",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_HardDiskId",
                table: "Assets",
                column: "HardDiskId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_KeyboardId",
                table: "Assets",
                column: "KeyboardId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MemoryDetailsId",
                table: "Assets",
                column: "MemoryDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MobileDetailsId",
                table: "Assets",
                column: "MobileDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MonitorId",
                table: "Assets",
                column: "MonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MouseId",
                table: "Assets",
                column: "MouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_NetworkDetailsId",
                table: "Assets",
                column: "NetworkDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_OperatingSystemInfoId",
                table: "Assets",
                column: "OperatingSystemInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProcessorId",
                table: "Assets",
                column: "ProcessorId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ProductId",
                table: "Assets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VendorId",
                table: "Assets",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualHostId",
                table: "Assets",
                column: "VirtualHostId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_ComputerInfoId",
                table: "Assets",
                column: "VirtualMachine_ComputerInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_HardDiskId",
                table: "Assets",
                column: "VirtualMachine_HardDiskId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_KeyboardId",
                table: "Assets",
                column: "VirtualMachine_KeyboardId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_MemoryDetailsId",
                table: "Assets",
                column: "VirtualMachine_MemoryDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_MonitorId",
                table: "Assets",
                column: "VirtualMachine_MonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_MouseId",
                table: "Assets",
                column: "VirtualMachine_MouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_OperatingSystemInfoId",
                table: "Assets",
                column: "VirtualMachine_OperatingSystemInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_VirtualMachine_ProcessorId",
                table: "Assets",
                column: "VirtualMachine_ProcessorId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AssetStates");

            migrationBuilder.DropTable(
                name: "ComputerInfos");

            migrationBuilder.DropTable(
                name: "HardDisks");

            migrationBuilder.DropTable(
                name: "Keyboards");

            migrationBuilder.DropTable(
                name: "MemoryDetails");

            migrationBuilder.DropTable(
                name: "Mice");

            migrationBuilder.DropTable(
                name: "MobileDetails");

            migrationBuilder.DropTable(
                name: "Monitors");

            migrationBuilder.DropTable(
                name: "NetworkDetails");

            migrationBuilder.DropTable(
                name: "OperatingSystemInfos");

            migrationBuilder.DropTable(
                name: "Processors");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Vendors");
        }
    }
}
