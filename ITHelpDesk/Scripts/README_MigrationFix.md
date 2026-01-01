# Fixing Migration History Sync Issue

## Problem
The database has some tables (like `AspNetRoles`) that already exist, but the migration history table (`__EFMigrationsHistory`) doesn't show that the migrations have been applied. This causes `dotnet ef database update` to fail when trying to create tables that already exist.

## Solution

You have two options to fix this:

### Option 1: Run the SQL Script (Recommended)

1. **Open SQL Server Management Studio (SSMS)** or use `sqlcmd`
2. **Connect to your database** using the connection string from `appsettings.json`:
   - Server: `localhost,1433`
   - Database: `ITHelpDesk`
   - User: `sa`
   - Password: `StrongPassword123!`

3. **Run the SQL script** `MarkMigrationsAsApplied.sql`:
   ```sql
   -- The script will check which tables exist and mark the corresponding migrations as applied
   ```

4. **After running the script**, run:
   ```powershell
   dotnet ef database update
   ```

### Option 2: Use PowerShell to Run the Script

If you have `sqlcmd` installed and SQL Server is accessible, you can run:

```powershell
cd ITHelpDesk
$connectionString = "Server=localhost,1433;Database=ITHelpDesk;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;"
$scriptPath = "Scripts\MarkMigrationsAsApplied.sql"

# Extract connection details and run script
sqlcmd -S localhost,1433 -d ITHelpDesk -U sa -P "StrongPassword123!" -i $scriptPath -C
```

### Option 3: Manual SQL (If scripts don't work)

If the above options don't work, you can manually run these SQL commands in SSMS:

```sql
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

-- Mark migrations as applied (only run the ones for tables that exist)
-- Check if AspNetRoles exists, then mark IdentityAndModels as applied
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[AspNetRoles]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110133406_IdentityAndModels')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110133406_IdentityAndModels', '8.0.21');
END
GO

-- Check if Tickets exists, then mark AddTicketsModel as applied
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Tickets]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251110191347_AddTicketsModel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251110191347_AddTicketsModel', '8.0.21');
END
GO

-- Repeat for other migrations as needed...
```

## After Fixing

Once you've marked the existing migrations as applied, run:

```powershell
cd ITHelpDesk
dotnet ef database update
```

This will apply only the remaining migrations that haven't been applied yet.

## Verification

To verify which migrations are applied, you can run:

```sql
SELECT * FROM [__EFMigrationsHistory] ORDER BY [MigrationId];
```

Or use EF Core:

```powershell
dotnet ef migrations list
```

