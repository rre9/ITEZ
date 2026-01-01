-- Script to mark migrations as applied if their tables already exist
-- This fixes the issue where tables exist but migration history is out of sync

USE ITHelpDesk;
GO

-- Ensure __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Mark IdentityAndModels as applied if AspNetRoles table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AspNetRoles]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110133406_IdentityAndModels')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110133406_IdentityAndModels', '8.0.21');
    PRINT 'Marked 20251110133406_IdentityAndModels as applied';
END
GO

-- Mark InitialCreate as applied if it created any tables (check for common initial tables)
-- Note: Adjust this based on what InitialCreate actually creates
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AspNetUsers]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110190125_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110190125_InitialCreate', '8.0.21');
    PRINT 'Marked 20251110190125_InitialCreate as applied';
END
GO

-- Mark AddTicketsModel as applied if Tickets table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Tickets]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110191347_AddTicketsModel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110191347_AddTicketsModel', '8.0.21');
    PRINT 'Marked 20251110191347_AddTicketsModel as applied';
END
GO

-- Mark AddAccessRequestsTable as applied if AccessRequests table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AccessRequests]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251224084014_AddAccessRequestsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251224084014_AddAccessRequestsTable', '8.0.21');
    PRINT 'Marked 20251224084014_AddAccessRequestsTable as applied';
END
GO

-- Mark AddSelectedManagerToAccessRequest as applied if AccessRequests table has SelectedManagerId column
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[AccessRequests]') AND name = 'SelectedManagerId')
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251224091913_AddSelectedManagerToAccessRequest')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251224091913_AddSelectedManagerToAccessRequest', '8.0.21');
    PRINT 'Marked 20251224091913_AddSelectedManagerToAccessRequest as applied';
END
GO

-- Mark AddServiceRequestsTable as applied if ServiceRequests table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ServiceRequests]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251229100948_AddServiceRequestsTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251229100948_AddServiceRequestsTable', '8.0.21');
    PRINT 'Marked 20251229100948_AddServiceRequestsTable as applied';
END
GO

-- Mark AddSystemChangeRequests as applied if SystemChangeRequests table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SystemChangeRequests]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251230201228_AddSystemChangeRequests')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251230201228_AddSystemChangeRequests', '8.0.21');
    PRINT 'Marked 20251230201228_AddSystemChangeRequests as applied';
END
GO

PRINT 'Migration history sync completed. You can now run: dotnet ef database update';
GO

