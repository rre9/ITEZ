-- Script to fix CloseReason column type from int to nvarchar(20)
-- Run this if the migration was already applied with int type

USE ITHEL;
GO

-- Check if column exists and is int type
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tickets]') AND name = 'CloseReason' AND system_type_id = 56) -- 56 = int
BEGIN
    -- Drop the column if it exists as int
    ALTER TABLE [Tickets] DROP COLUMN [CloseReason];
    PRINT 'Dropped CloseReason column (int type)';
END
GO

-- Add the column as nvarchar(20)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tickets]') AND name = 'CloseReason')
BEGIN
    ALTER TABLE [Tickets] ADD [CloseReason] nvarchar(20) NULL;
    PRINT 'Added CloseReason column (nvarchar(20) type)';
END
ELSE
BEGIN
    -- If column exists but is not nvarchar, alter it
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Tickets]') AND name = 'CloseReason' AND system_type_id != 231) -- 231 = nvarchar
    BEGIN
        ALTER TABLE [Tickets] ALTER COLUMN [CloseReason] nvarchar(20) NULL;
        PRINT 'Altered CloseReason column to nvarchar(20)';
    END
    ELSE
    BEGIN
        PRINT 'CloseReason column already exists as nvarchar(20)';
    END
END
GO

PRINT 'CloseReason column type fix completed.';
GO

