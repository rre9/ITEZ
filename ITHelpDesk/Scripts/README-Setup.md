# OCI Windows Server Setup Guide

This guide walks you through setting up your IT Help Desk application on an OCI Windows Server.

## Prerequisites

- OCI Windows Server 2019/2022 with:
  - 2-4 vCPU
  - 8-16 GB RAM
  - ~300 GB storage
- Administrator access to the server
- RDP access to the server

## Quick Start

### Option 1: Automated Setup (Recommended)

1. **Connect to your OCI Windows Server via RDP**

2. **Download or copy the scripts to the server**
   - Copy the entire `Scripts` folder to `C:\Setup\` on your server

3. **Run the master setup script:**
   ```powershell
   # Open PowerShell as Administrator
   cd C:\Setup\Scripts
   .\Setup-OCIServer.ps1
   ```

   The script will guide you through:
   - SQL Server installation (manual step required)
   - SQL Server configuration
   - .NET 8.0 Hosting Bundle installation
   - IIS installation and configuration
   - Database setup

### Option 2: Manual Step-by-Step Setup

Follow the steps below if you prefer manual control.

---

## Step-by-Step Setup

### Step 1: Install SQL Server 2022 Express

1. **Download SQL Server 2022 Express:**
   - Visit: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Download "Express" edition

2. **Run the installer:**
   - Select "Basic" installation type (or "Custom" for more control)
   - **Important:** During installation:
     - Choose "Mixed Mode Authentication"
     - Set a strong password for the `sa` account
     - Note the password for later use

3. **Verify installation:**
   ```powershell
   Get-Service -Name "MSSQLSERVER"
   ```

4. **Run SQL Server configuration:**
   ```powershell
   .\Configure-SQLServer.ps1
   ```

### Step 2: Configure SQL Server

The `Configure-SQLServer.ps1` script will:
- Enable TCP/IP protocol
- Configure firewall rules
- Set up SQL Server Browser

**Manual steps required:**
1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Right-click server → Properties → Security
4. Select "SQL Server and Windows Authentication mode"
5. Click OK and restart SQL Server service

### Step 3: Install .NET 8.0 Hosting Bundle

```powershell
.\Install-DotNetHosting.ps1
```

This script will:
- Download .NET 8.0 Hosting Bundle
- Install it automatically
- Verify installation

### Step 4: Install and Configure IIS

```powershell
.\Install-ConfigureIIS.ps1
```

This script will:
- Install IIS with required features
- Create application pool for your application
- Configure IIS for ASP.NET Core

### Step 5: Set Up Database

1. **Run the database setup script:**
   ```powershell
   .\Setup-Database.ps1 -DatabaseUserPassword "YourStrongPassword123!"
   ```

   Or run the SQL script manually in SSMS:
   - Open `CreateDatabase_OCI.sql`
   - Update the password in the script
   - Execute the script

### Step 6: Deploy Application

1. **Publish your application:**
   ```powershell
   # From your development machine
   dotnet publish -c Release -o C:\Publish\ITHelpDesk
   ```

2. **Copy published files to server:**
   - Copy the published folder to `C:\inetpub\wwwroot\ITHelpDesk` on the server

3. **Run deployment script:**
   ```powershell
   .\Deploy-Application.ps1
   ```

   Or manually:
   ```powershell
   .\Deploy-Application.ps1 -SkipPublish -PublishPath "C:\inetpub\wwwroot\ITHelpDesk"
   ```

### Step 7: Configure Connection String

```powershell
.\Configure-ConnectionString.ps1
```

This will:
- Prompt for database connection details
- Set environment variable (recommended) or update appsettings.Production.json
- Optionally test the connection

**After configuration, restart IIS:**
```powershell
Restart-WebAppPool -Name "ITHelpDeskAppPool"
iisreset
```

---

## OCI Network Configuration

### Security List / Network Security Group Rules

Configure your OCI Security List to allow:

| Type | Protocol | Port | Source | Description |
|------|----------|------|--------|-------------|
| Ingress | TCP | 80 | 0.0.0.0/0 | HTTP |
| Ingress | TCP | 443 | 0.0.0.0/0 | HTTPS |
| Ingress | TCP | 1433 | App Server IP | SQL Server |
| Ingress | TCP | 3389 | Your IP | RDP (for management) |

**Important:** Restrict SQL Server port (1433) to only your application server IP for security.

---

## Verification Checklist

After setup, verify:

- [ ] SQL Server service is running
- [ ] SQL Server accepts TCP/IP connections on port 1433
- [ ] Database `ITHelpDesk` exists
- [ ] User `ithelpdesk_user` exists and has permissions
- [ ] .NET 8.0 runtime is installed (`dotnet --list-runtimes`)
- [ ] IIS is running and website is started
- [ ] Application is accessible at `http://localhost` or `http://your-server-ip`
- [ ] Connection string is configured correctly
- [ ] Application can connect to database

---

## Troubleshooting

### SQL Server Connection Issues

1. **Check SQL Server is running:**
   ```powershell
   Get-Service -Name "MSSQLSERVER"
   ```

2. **Check TCP/IP is enabled:**
   - Open SQL Server Configuration Manager
   - SQL Server Network Configuration → Protocols
   - Ensure TCP/IP is enabled

3. **Check firewall:**
   ```powershell
   Get-NetFirewallRule -DisplayName "SQL Server"
   ```

4. **Test connection:**
   ```powershell
   sqlcmd -S localhost,1433 -U ithelpdesk_user -P YourPassword -Q "SELECT 1"
   ```

### Application Not Starting

1. **Check IIS logs:**
   - `C:\inetpub\logs\LogFiles\`

2. **Check application logs:**
   - Check `DataProtectionKeys` folder permissions
   - Verify connection string is set correctly

3. **Check application pool:**
   ```powershell
   Get-WebAppPoolState -Name "ITHelpDeskAppPool"
   ```

4. **Restart application pool:**
   ```powershell
   Restart-WebAppPool -Name "ITHelpDeskAppPool"
   ```

### Database Migration Issues

If migrations fail on startup:

1. **Run migrations manually:**
   ```powershell
   cd C:\inetpub\wwwroot\ITHelpDesk
   dotnet ef database update
   ```

2. **Check database permissions:**
   - Ensure `ithelpdesk_user` has `db_owner` role

---

## Security Recommendations

1. **Change default passwords:**
   - SQL Server `sa` account
   - Database user `ithelpdesk_user`

2. **Use environment variables for connection strings:**
   - More secure than appsettings.json
   - Use `Configure-ConnectionString.ps1` with default options

3. **Enable HTTPS:**
   - Install SSL certificate
   - Configure IIS bindings for HTTPS

4. **Regular backups:**
   - Set up automated SQL Server backups
   - Test restore procedures

5. **Keep software updated:**
   - Windows Server updates
   - SQL Server updates
   - .NET runtime updates

---

## Next Steps

After successful setup:

1. **Configure SSL/HTTPS** for production
2. **Set up automated backups** for the database
3. **Configure monitoring** and logging
4. **Test the application** thoroughly
5. **Set up staging environment** (recommended)

---

## Support

For issues:
1. Check application logs
2. Check SQL Server error logs
3. Check Windows Event Viewer
4. Check IIS logs

Scripts location: `C:\Setup\Scripts\` (or wherever you placed them)

