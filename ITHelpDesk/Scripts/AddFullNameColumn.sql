USE ITHEL;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Check if column exists, if not add it
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') 
               AND name = 'FullName')
BEGIN
    ALTER TABLE [AspNetUsers] 
    ADD [FullName] nvarchar(150) NOT NULL DEFAULT '';
    PRINT 'Column FullName added successfully';
END
ELSE
BEGIN
    PRINT 'Column FullName already exists';
END
GO

