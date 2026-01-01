-- Script to drop the ITHelpDesk database
-- This will delete all data and tables

USE master;
GO

-- Close existing connections to the database
ALTER DATABASE [ITHelpDesk] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Drop the database
DROP DATABASE [ITHelpDesk];
GO

PRINT 'Database ITHelpDesk has been dropped successfully.';
GO

