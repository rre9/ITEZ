CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AssetStates] (
    [Id] int NOT NULL IDENTITY,
    [Status] int NOT NULL,
    [AssociatedTo] nvarchar(50) NULL,
    [Site] nvarchar(50) NULL,
    [StateComments] nvarchar(500) NULL,
    [UserId] nvarchar(450) NULL,
    [Department] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_AssetStates] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [ComputerInfos] (
    [Id] int NOT NULL IDENTITY,
    [ServiceTag] nvarchar(50) NULL,
    [Manufacturer] nvarchar(100) NULL,
    [BiosDate] datetime2 NULL,
    [Domain] nvarchar(100) NULL,
    [SMBiosVersion] nvarchar(50) NULL,
    [BiosVersion] nvarchar(50) NULL,
    [BiosManufacturer] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ComputerInfos] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [HardDisks] (
    [Id] int NOT NULL IDENTITY,
    [Model] nvarchar(100) NULL,
    [SerialNumber] nvarchar(100) NULL,
    [Manufacturer] nvarchar(100) NULL,
    [CapacityGB] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_HardDisks] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Keyboards] (
    [Id] int NOT NULL IDENTITY,
    [KeyboardType] nvarchar(100) NULL,
    [Manufacturer] nvarchar(100) NULL,
    [SerialNumber] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Keyboards] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MemoryDetails] (
    [Id] int NOT NULL IDENTITY,
    [RAM] int NULL,
    [VirtualMemory] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_MemoryDetails] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Mice] (
    [Id] int NOT NULL IDENTITY,
    [MouseType] nvarchar(100) NULL,
    [SerialNumber] nvarchar(100) NULL,
    [MouseButtons] int NULL,
    [Manufacturer] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Mice] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MobileDetails] (
    [Id] int NOT NULL IDENTITY,
    [IMEI] nvarchar(20) NULL,
    [Model] nvarchar(100) NULL,
    [ModelNo] nvarchar(100) NULL,
    [TotalCapacityGB] int NULL,
    [AvailableCapacityGB] int NULL,
    [ModemFirmwareVersion] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_MobileDetails] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Monitors] (
    [Id] int NOT NULL IDENTITY,
    [MonitorType] nvarchar(100) NULL,
    [SerialNumber] nvarchar(100) NULL,
    [Manufacturer] nvarchar(100) NULL,
    [MaxResolution] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Monitors] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [NetworkDetails] (
    [Id] int NOT NULL IDENTITY,
    [IPAddress] nvarchar(15) NULL,
    [MACAddress] nvarchar(17) NULL,
    [NIC] nvarchar(50) NULL,
    [Network] nvarchar(100) NULL,
    [DefaultGateway] nvarchar(15) NULL,
    [DHCPEnabled] bit NOT NULL,
    [DHCPServer] nvarchar(15) NULL,
    [DNSHostname] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_NetworkDetails] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [OperatingSystemInfos] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NULL,
    [Version] nvarchar(50) NULL,
    [BuildNumber] nvarchar(50) NULL,
    [ServicePack] nvarchar(50) NULL,
    [ProductId] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_OperatingSystemInfos] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Processors] (
    [Id] int NOT NULL IDENTITY,
    [ProcessorInfo] nvarchar(150) NULL,
    [Manufacturer] nvarchar(100) NULL,
    [ClockSpeedMHz] int NULL,
    [NumberOfCores] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Processors] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [ProductType] nvarchar(100) NOT NULL,
    [ProductName] nvarchar(150) NOT NULL,
    [Manufacturer] nvarchar(100) NOT NULL,
    [PartNo] nvarchar(50) NULL,
    [Cost] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Vendors] (
    [Id] int NOT NULL IDENTITY,
    [VendorName] nvarchar(150) NOT NULL,
    [Currency] nvarchar(5) NOT NULL,
    [DoorNumber] nvarchar(10) NULL,
    [Landmark] nvarchar(100) NULL,
    [PostalCode] nvarchar(10) NULL,
    [Country] nvarchar(50) NULL,
    [Fax] nvarchar(20) NULL,
    [FirstName] nvarchar(50) NULL,
    [Street] nvarchar(100) NULL,
    [City] nvarchar(50) NULL,
    [StateProvince] nvarchar(50) NULL,
    [PhoneNo] nvarchar(20) NULL,
    [Email] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Tickets] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Priority] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedById] nvarchar(450) NOT NULL,
    [AssignedToId] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tickets_AspNetUsers_AssignedToId] FOREIGN KEY ([AssignedToId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Tickets_AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Assets] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(150) NOT NULL,
    [ProductId] int NOT NULL,
    [SerialNumber] nvarchar(100) NULL,
    [AssetTag] nvarchar(50) NULL,
    [VendorId] int NULL,
    [PurchaseCost] decimal(18,2) NOT NULL,
    [ExpiryDate] datetime2 NULL,
    [Location] nvarchar(150) NULL,
    [AcquisitionDate] datetime2 NULL,
    [WarrantyExpiryDate] datetime2 NULL,
    [AssetStateId] int NULL,
    [NetworkDetailsId] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] datetime2 NULL,
    [CreatedById] nvarchar(450) NULL,
    [AssetType] nvarchar(21) NOT NULL,
    [ComputerInfoId] int NULL,
    [OperatingSystemInfoId] int NULL,
    [MemoryDetailsId] int NULL,
    [ProcessorId] int NULL,
    [HardDiskId] int NULL,
    [KeyboardId] int NULL,
    [MouseId] int NULL,
    [MonitorId] int NULL,
    [MobileDetailsId] int NULL,
    [VMPlatform] nvarchar(100) NULL,
    [VirtualHostId] int NULL,
    [VirtualMachine_ComputerInfoId] int NULL,
    [VirtualMachine_OperatingSystemInfoId] int NULL,
    [VirtualMachine_MemoryDetailsId] int NULL,
    [VirtualMachine_ProcessorId] int NULL,
    [VirtualMachine_HardDiskId] int NULL,
    [VirtualMachine_KeyboardId] int NULL,
    [VirtualMachine_MouseId] int NULL,
    [VirtualMachine_MonitorId] int NULL,
    CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assets_AspNetUsers_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assets_AssetStates_AssetStateId] FOREIGN KEY ([AssetStateId]) REFERENCES [AssetStates] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Assets_VirtualHostId] FOREIGN KEY ([VirtualHostId]) REFERENCES [Assets] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assets_ComputerInfos_ComputerInfoId] FOREIGN KEY ([ComputerInfoId]) REFERENCES [ComputerInfos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_ComputerInfos_VirtualMachine_ComputerInfoId] FOREIGN KEY ([VirtualMachine_ComputerInfoId]) REFERENCES [ComputerInfos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_HardDisks_HardDiskId] FOREIGN KEY ([HardDiskId]) REFERENCES [HardDisks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_HardDisks_VirtualMachine_HardDiskId] FOREIGN KEY ([VirtualMachine_HardDiskId]) REFERENCES [HardDisks] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Keyboards_KeyboardId] FOREIGN KEY ([KeyboardId]) REFERENCES [Keyboards] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Keyboards_VirtualMachine_KeyboardId] FOREIGN KEY ([VirtualMachine_KeyboardId]) REFERENCES [Keyboards] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_MemoryDetails_MemoryDetailsId] FOREIGN KEY ([MemoryDetailsId]) REFERENCES [MemoryDetails] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_MemoryDetails_VirtualMachine_MemoryDetailsId] FOREIGN KEY ([VirtualMachine_MemoryDetailsId]) REFERENCES [MemoryDetails] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Mice_MouseId] FOREIGN KEY ([MouseId]) REFERENCES [Mice] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Mice_VirtualMachine_MouseId] FOREIGN KEY ([VirtualMachine_MouseId]) REFERENCES [Mice] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_MobileDetails_MobileDetailsId] FOREIGN KEY ([MobileDetailsId]) REFERENCES [MobileDetails] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Monitors_MonitorId] FOREIGN KEY ([MonitorId]) REFERENCES [Monitors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Monitors_VirtualMachine_MonitorId] FOREIGN KEY ([VirtualMachine_MonitorId]) REFERENCES [Monitors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_NetworkDetails_NetworkDetailsId] FOREIGN KEY ([NetworkDetailsId]) REFERENCES [NetworkDetails] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_OperatingSystemInfos_OperatingSystemInfoId] FOREIGN KEY ([OperatingSystemInfoId]) REFERENCES [OperatingSystemInfos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_OperatingSystemInfos_VirtualMachine_OperatingSystemInfoId] FOREIGN KEY ([VirtualMachine_OperatingSystemInfoId]) REFERENCES [OperatingSystemInfos] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Processors_ProcessorId] FOREIGN KEY ([ProcessorId]) REFERENCES [Processors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Processors_VirtualMachine_ProcessorId] FOREIGN KEY ([VirtualMachine_ProcessorId]) REFERENCES [Processors] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Assets_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assets_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [AccessRequests] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [FullName] nvarchar(150) NOT NULL,
    [EmployeeNumber] nvarchar(50) NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(50) NULL,
    [AccessType] nvarchar(20) NOT NULL,
    [SystemName] nvarchar(150) NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [AccessDuration] nvarchar(100) NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [SelectedManagerId] nvarchar(450) NOT NULL,
    [ManagerApprovalName] nvarchar(150) NULL,
    [ManagerApprovalDate] datetime2 NULL,
    [ManagerApprovalStatus] nvarchar(20) NOT NULL,
    [ITApprovalName] nvarchar(150) NULL,
    [ITApprovalDate] datetime2 NULL,
    [ITApprovalStatus] nvarchar(20) NOT NULL,
    [SecurityApprovalName] nvarchar(150) NULL,
    [SecurityApprovalDate] datetime2 NULL,
    [SecurityApprovalStatus] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_AccessRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AccessRequests_AspNetUsers_SelectedManagerId] FOREIGN KEY ([SelectedManagerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AccessRequests_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ServiceRequests] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [EmployeeName] nvarchar(150) NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [JobTitle] nvarchar(100) NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    [UsageDescription] nvarchar(2000) NOT NULL,
    [UsageReason] nvarchar(2000) NOT NULL,
    [Acknowledged] bit NOT NULL,
    [SignatureName] nvarchar(150) NOT NULL,
    [SignatureJobTitle] nvarchar(100) NOT NULL,
    [SignatureDate] datetime2 NOT NULL,
    [SelectedManagerId] nvarchar(450) NOT NULL,
    [ManagerApprovalName] nvarchar(150) NULL,
    [ManagerApprovalDate] datetime2 NULL,
    [ManagerApprovalStatus] nvarchar(20) NOT NULL,
    [ITApprovalName] nvarchar(150) NULL,
    [ITApprovalDate] datetime2 NULL,
    [ITApprovalStatus] nvarchar(20) NOT NULL,
    [SecurityApprovalName] nvarchar(150) NULL,
    [SecurityApprovalDate] datetime2 NULL,
    [SecurityApprovalStatus] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_ServiceRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServiceRequests_AspNetUsers_SelectedManagerId] FOREIGN KEY ([SelectedManagerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ServiceRequests_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [TicketAttachments] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [FileName] nvarchar(255) NOT NULL,
    [FilePath] nvarchar(500) NOT NULL,
    [UploadTime] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_TicketAttachments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketAttachments_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [TicketLogs] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [Action] nvarchar(150) NOT NULL,
    [Notes] nvarchar(1000) NULL,
    [PerformedById] nvarchar(450) NOT NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_TicketLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketLogs_AspNetUsers_PerformedById] FOREIGN KEY ([PerformedById]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TicketLogs_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AccessRequests_SelectedManagerId] ON [AccessRequests] ([SelectedManagerId]);
GO


CREATE UNIQUE INDEX [IX_AccessRequests_TicketId] ON [AccessRequests] ([TicketId]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Assets_AssetStateId] ON [Assets] ([AssetStateId]);
GO


CREATE INDEX [IX_Assets_ComputerInfoId] ON [Assets] ([ComputerInfoId]);
GO


CREATE INDEX [IX_Assets_CreatedById] ON [Assets] ([CreatedById]);
GO


CREATE INDEX [IX_Assets_HardDiskId] ON [Assets] ([HardDiskId]);
GO


CREATE INDEX [IX_Assets_KeyboardId] ON [Assets] ([KeyboardId]);
GO


CREATE INDEX [IX_Assets_MemoryDetailsId] ON [Assets] ([MemoryDetailsId]);
GO


CREATE INDEX [IX_Assets_MobileDetailsId] ON [Assets] ([MobileDetailsId]);
GO


CREATE INDEX [IX_Assets_MonitorId] ON [Assets] ([MonitorId]);
GO


CREATE INDEX [IX_Assets_MouseId] ON [Assets] ([MouseId]);
GO


CREATE INDEX [IX_Assets_NetworkDetailsId] ON [Assets] ([NetworkDetailsId]);
GO


CREATE INDEX [IX_Assets_OperatingSystemInfoId] ON [Assets] ([OperatingSystemInfoId]);
GO


CREATE INDEX [IX_Assets_ProcessorId] ON [Assets] ([ProcessorId]);
GO


CREATE INDEX [IX_Assets_ProductId] ON [Assets] ([ProductId]);
GO


CREATE INDEX [IX_Assets_VendorId] ON [Assets] ([VendorId]);
GO


CREATE INDEX [IX_Assets_VirtualHostId] ON [Assets] ([VirtualHostId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_ComputerInfoId] ON [Assets] ([VirtualMachine_ComputerInfoId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_HardDiskId] ON [Assets] ([VirtualMachine_HardDiskId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_KeyboardId] ON [Assets] ([VirtualMachine_KeyboardId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_MemoryDetailsId] ON [Assets] ([VirtualMachine_MemoryDetailsId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_MonitorId] ON [Assets] ([VirtualMachine_MonitorId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_MouseId] ON [Assets] ([VirtualMachine_MouseId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_OperatingSystemInfoId] ON [Assets] ([VirtualMachine_OperatingSystemInfoId]);
GO


CREATE INDEX [IX_Assets_VirtualMachine_ProcessorId] ON [Assets] ([VirtualMachine_ProcessorId]);
GO


CREATE INDEX [IX_ServiceRequests_SelectedManagerId] ON [ServiceRequests] ([SelectedManagerId]);
GO


CREATE UNIQUE INDEX [IX_ServiceRequests_TicketId] ON [ServiceRequests] ([TicketId]);
GO


CREATE INDEX [IX_TicketAttachments_TicketId] ON [TicketAttachments] ([TicketId]);
GO


CREATE INDEX [IX_TicketLogs_PerformedById] ON [TicketLogs] ([PerformedById]);
GO


CREATE INDEX [IX_TicketLogs_TicketId] ON [TicketLogs] ([TicketId]);
GO


CREATE INDEX [IX_Tickets_AssignedToId] ON [Tickets] ([AssignedToId]);
GO


CREATE INDEX [IX_Tickets_CreatedById] ON [Tickets] ([CreatedById]);
GO


