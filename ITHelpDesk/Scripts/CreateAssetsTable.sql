-- Create Assets table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Assets]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Assets](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [AssetType] [nvarchar](100) NOT NULL,
        [Name] [nvarchar](150) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [SerialNumber] [nvarchar](100) NULL,
        [AssetTag] [nvarchar](100) NULL,
        [PurchaseDate] [datetime2](7) NULL,
        [PurchaseCost] [decimal](18, 2) NULL,
        [WarrantyExpiration] [datetime2](7) NULL,
        [Location] [nvarchar](200) NULL,
        [AssignedUserId] [nvarchar](450) NULL,
        [ProductId] [int] NULL,
        [VendorId] [int] NULL,
        [AssetStateId] [int] NULL,
        [NetworkDetailsId] [int] NULL,
        [ComputerInfoId] [int] NULL,
        [OperatingSystemInfoId] [int] NULL,
        [MemoryDetailsId] [int] NULL,
        [ProcessorId] [int] NULL,
        [HardDiskId] [int] NULL,
        [KeyboardId] [int] NULL,
        [MouseId] [int] NULL,
        [MonitorId] [int] NULL,
        [MobileDetailsId] [int] NULL,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT (GETUTCDATE()),
        [UpdatedAt] [datetime2](7) NULL,
     CONSTRAINT [PK_Assets] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
END
GO

-- Create indexes if they don't exist
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_AssetStateId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_AssetStateId] ON [dbo].[Assets]([AssetStateId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_AssignedUserId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_AssignedUserId] ON [dbo].[Assets]([AssignedUserId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_ComputerInfoId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_ComputerInfoId] ON [dbo].[Assets]([ComputerInfoId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_HardDiskId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLESTERED INDEX [IX_Assets_HardDiskId] ON [dbo].[Assets]([HardDiskId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_KeyboardId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_KeyboardId] ON [dbo].[Assets]([KeyboardId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_MemoryDetailsId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_MemoryDetailsId] ON [dbo].[Assets]([MemoryDetailsId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_MobileDetailsId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_MobileDetailsId] ON [dbo].[Assets]([MobileDetailsId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_MonitorId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_MonitorId] ON [dbo].[Assets]([MonitorId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_MouseId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_MouseId] ON [dbo].[Assets]([MouseId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_NetworkDetailsId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_NetworkDetailsId] ON [dbo].[Assets]([NetworkDetailsId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_OperatingSystemInfoId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_OperatingSystemInfoId] ON [dbo].[Assets]([OperatingSystemInfoId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_ProcessorId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_ProcessorId] ON [dbo].[Assets]([ProcessorId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_ProductId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_ProductId] ON [dbo].[Assets]([ProductId] ASC)
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assets_VendorId' AND object_id = OBJECT_ID('Assets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Assets_VendorId] ON [dbo].[Assets]([VendorId] ASC)
END
GO

PRINT 'Assets table and indexes created successfully'
