-- Script to verify that all required tables exist
USE ITHEL;
GO

PRINT 'Checking for required tables...';
PRINT '';

-- Check for Tickets table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Tickets]') AND type in (N'U'))
BEGIN
    PRINT '✓ Tickets table exists';
END
ELSE
BEGIN
    PRINT '✗ Tickets table is MISSING';
END
GO

-- Check for AccessRequests table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AccessRequests]') AND type in (N'U'))
BEGIN
    PRINT '✓ AccessRequests table exists';
END
ELSE
BEGIN
    PRINT '✗ AccessRequests table is MISSING';
END
GO

-- Check for ServiceRequests table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ServiceRequests]') AND type in (N'U'))
BEGIN
    PRINT '✓ ServiceRequests table exists';
END
ELSE
BEGIN
    PRINT '✗ ServiceRequests table is MISSING';
END
GO

-- Check for SystemChangeRequests table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SystemChangeRequests]') AND type in (N'U'))
BEGIN
    PRINT '✓ SystemChangeRequests table exists';
END
ELSE
BEGIN
    PRINT '✗ SystemChangeRequests table is MISSING';
END
GO

-- Check for __EFMigrationsHistory table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    PRINT '✓ __EFMigrationsHistory table exists';
    PRINT '';
    PRINT 'Applied migrations:';
    SELECT [MigrationId], [ProductVersion] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
END
ELSE
BEGIN
    PRINT '✗ __EFMigrationsHistory table is MISSING';
END
GO

PRINT '';
PRINT 'Verification completed.';
GO

