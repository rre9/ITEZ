# Quick Start Guide - OCI Windows Server Setup

## 🚀 Fast Track Setup (30-45 minutes)

### Prerequisites
- OCI Windows Server 2019/2022 (2-4 vCPU, 8-16 GB RAM, 300 GB storage)
- Administrator access via RDP
- Scripts folder copied to server (e.g., `C:\Setup\Scripts`)

---

## Step 1: Install SQL Server 2022 Express (15 minutes)

1. **Download:**
   - https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Select "Express" edition

2. **Install:**
   - Run installer
   - Choose "Basic" installation
   - **IMPORTANT:** Select "Mixed Mode Authentication"
   - Set strong `sa` password (save it!)

3. **Verify:**
   ```powershell
   Get-Service -Name "MSSQLSERVER"
   ```

---

## Step 2: Run Automated Setup Scripts (10 minutes)

Open PowerShell as Administrator on your OCI server:

```powershell
cd C:\Setup\Scripts

# Configure SQL Server (enables TCP/IP, firewall)
.\Configure-SQLServer.ps1

# Install .NET 8.0 Hosting Bundle
.\Install-DotNetHosting.ps1

# Install and configure IIS
.\Install-ConfigureIIS.ps1

# Setup database (you'll be prompted for password)
.\Setup-Database.ps1 -DatabaseUserPassword "YourStrongPassword123!"
```

**Manual Step Required:** After `Configure-SQLServer.ps1`, enable Mixed Mode Authentication in SSMS:
1. Open SSMS → Connect to server
2. Right-click server → Properties → Security
3. Select "SQL Server and Windows Authentication mode"
4. Restart SQL Server service

---

## Step 3: Deploy Application (10 minutes)

### Option A: From Development Machine

1. **Publish application:**
   ```powershell
   # On your dev machine
   cd C:\Projects\EZIT\ITHelpDesk
   dotnet publish -c Release -o C:\Publish\ITHelpDesk
   ```

2. **Copy to server:**
   - Copy `C:\Publish\ITHelpDesk` folder to server at `C:\inetpub\wwwroot\ITHelpDesk`

3. **Deploy on server:**
   ```powershell
   # On OCI server
   cd C:\Setup\Scripts
   .\Deploy-Application.ps1 -SkipPublish -PublishPath "C:\inetpub\wwwroot\ITHelpDesk"
   ```

### Option B: Direct on Server

If you have the source code on the server:

```powershell
cd C:\Projects\EZIT\ITHelpDesk
.\Scripts\Deploy-Application.ps1
```

---

## Step 4: Configure Connection String (5 minutes)

```powershell
cd C:\Setup\Scripts
.\Configure-ConnectionString.ps1
```

**Enter when prompted:**
- Server: `localhost` (or your SQL Server IP)
- Port: `1433`
- Database: `ITHelpDesk`
- Username: `ithelpdesk_user`
- Password: (the one you set in Step 2)

**Restart IIS:**
```powershell
Restart-WebAppPool -Name "ITHelpDeskAppPool"
iisreset
```

---

## Step 5: Test Application

1. **Open browser:**
   - `http://localhost` or `http://your-server-ip`

2. **Verify:**
   - Application loads
   - Can register/login
   - Database connection works

---

## ✅ Verification Checklist

Run these commands to verify setup:

```powershell
# Check SQL Server
Get-Service -Name "MSSQLSERVER"

# Check .NET 8.0
dotnet --list-runtimes

# Check IIS
Get-Service -Name "W3SVC"
Get-Website -Name "ITHelpDesk"

# Check application pool
Get-WebAppPoolState -Name "ITHelpDeskAppPool"

# Test database connection
sqlcmd -S localhost,1433 -U ithelpdesk_user -P YourPassword -Q "SELECT DB_NAME()"
```

---

## 🔧 Common Issues & Quick Fixes

### SQL Server not accessible
```powershell
# Check TCP/IP is enabled
# Open SQL Server Configuration Manager → Enable TCP/IP

# Restart SQL Server
Restart-Service -Name "MSSQLSERVER"
```

### Application won't start
```powershell
# Check application pool
Get-WebAppPoolState -Name "ITHelpDeskAppPool"

# Restart application pool
Restart-WebAppPool -Name "ITHelpDeskAppPool"

# Check logs
Get-Content C:\inetpub\wwwroot\ITHelpDesk\logs\stdout*.log -Tail 50
```

### Database connection fails
```powershell
# Test connection
sqlcmd -S localhost,1433 -U ithelpdesk_user -P YourPassword -Q "SELECT 1"

# Verify environment variable
[System.Environment]::GetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Machine")
```

---

## 📋 OCI Network Configuration

**Security List Rules (Required):**

| Type | Protocol | Port | Source | Description |
|------|----------|------|--------|-------------|
| Ingress | TCP | 80 | 0.0.0.0/0 | HTTP |
| Ingress | TCP | 443 | 0.0.0.0/0 | HTTPS |
| Ingress | TCP | 1433 | App Server IP | SQL Server (restrict!) |
| Ingress | TCP | 3389 | Your IP | RDP |

---

## 🎯 Next Steps After Setup

1. **Configure HTTPS/SSL** for production
2. **Set up automated database backups**
3. **Configure email settings** in appsettings.Production.json
4. **Set up monitoring** and alerting
5. **Test all application features**

---

## 📞 Need Help?

1. Check `README-Setup.md` for detailed documentation
2. Review script outputs for error messages
3. Check Windows Event Viewer for system errors
4. Check IIS logs: `C:\inetpub\logs\LogFiles\`

---

**Estimated Total Time: 30-45 minutes**

