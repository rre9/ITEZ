-- Simple script to create Assets table
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'Assets')
BEGIN
    CREATE TABLE Assets
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AssetType NVARCHAR(100) NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,
        SerialNumber NVARCHAR(100) NULL,
        AssetTag NVARCHAR(100) NULL,
        PurchaseDate DATETIME2 NULL,
        PurchaseCost DECIMAL(18, 2) NULL,
        WarrantyExpiration DATETIME2 NULL,
        Location NVARCHAR(200) NULL,
        AssignedUserId NVARCHAR(450) NULL,
        ProductId INT NULL,
        VendorId INT NULL,
        AssetStateId INT NULL,
        NetworkDetailsId INT NULL,
        ComputerInfoId INT NULL,
        OperatingSystemInfoId INT NULL,
        MemoryDetailsId INT NULL,
        ProcessorId INT NULL,
        HardDiskId INT NULL,
        KeyboardId INT NULL,
        MouseId INT NULL,
        MonitorId INT NULL,
        MobileDetailsId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Assets table created successfully';
END
ELSE
BEGIN
    PRINT 'Assets table already exists';
END
GO
