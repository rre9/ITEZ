# =============================================
# OCI Windows Server Setup Script
# =============================================
# This script automates the setup of:
# 1. SQL Server 2022 Express
# 2. .NET 8.0 Hosting Bundle
# 3. IIS Configuration
# 4. Database Setup
# =============================================
# Run this script as Administrator on your OCI Windows Server
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$SqlServerPassword,
    
    [Parameter(Mandatory=$false)]
    [string]$DatabaseUserPassword,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipSqlServerInstall,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipDotNetInstall,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipIISInstall
)

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "OCI Windows Server Setup for IT Help Desk" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Step 1: SQL Server Installation
if (-not $SkipSqlServerInstall) {
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host "Step 1: SQL Server 2022 Express Setup" -ForegroundColor Yellow
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host ""
    
    # Check if SQL Server is already installed
    $sqlServerInstalled = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
    if ($sqlServerInstalled) {
        Write-Host "✅ SQL Server is already installed" -ForegroundColor Green
    } else {
        Write-Host "📥 SQL Server 2022 Express needs to be installed manually" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please download and install SQL Server 2022 Express from:" -ForegroundColor White
        Write-Host "https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "During installation, select:" -ForegroundColor White
        Write-Host "  - Mixed Mode Authentication" -ForegroundColor White
        Write-Host "  - Set a strong password for 'sa' account" -ForegroundColor White
        Write-Host "  - Enable TCP/IP protocol" -ForegroundColor White
        Write-Host ""
        
        $continue = Read-Host "Press Enter after SQL Server is installed, or 'Q' to quit"
        if ($continue -eq 'Q') { exit 1 }
        
        # Verify installation
        $sqlServerInstalled = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
        if (-not $sqlServerInstalled) {
            Write-Host "❌ SQL Server service not found. Please install SQL Server first." -ForegroundColor Red
            exit 1
        }
    }
    
    # Run SQL Server configuration script
    Write-Host "🔧 Configuring SQL Server..." -ForegroundColor Cyan
    & "$scriptPath\Configure-SQLServer.ps1" -SqlServerPassword $SqlServerPassword
}

# Step 2: .NET 8.0 Hosting Bundle
if (-not $SkipDotNetInstall) {
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host "Step 2: .NET 8.0 Hosting Bundle Setup" -ForegroundColor Yellow
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host ""
    
    & "$scriptPath\Install-DotNetHosting.ps1"
}

# Step 3: IIS Installation
if (-not $SkipIISInstall) {
    Write-Host ""
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host "Step 3: IIS Installation and Configuration" -ForegroundColor Yellow
    Write-Host "=============================================" -ForegroundColor Yellow
    Write-Host ""
    
    & "$scriptPath\Install-ConfigureIIS.ps1"
}

# Step 4: Database Setup
Write-Host ""
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host "Step 4: Database Setup" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow
Write-Host ""

if (-not $DatabaseUserPassword) {
    Write-Host "Enter password for database user 'ithelpdesk_user':" -ForegroundColor Yellow
    $securePassword = Read-Host "Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $DatabaseUserPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

& "$scriptPath\Setup-Database.ps1" -DatabaseUserPassword $DatabaseUserPassword

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "✅ Server Setup Complete!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Deploy your application using: Deploy-Application.ps1" -ForegroundColor White
Write-Host "2. Configure connection string using: Configure-ConnectionString.ps1" -ForegroundColor White
Write-Host "3. Test the application" -ForegroundColor White
Write-Host ""

