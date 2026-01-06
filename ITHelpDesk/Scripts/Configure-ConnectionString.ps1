# =============================================
# Connection String Configuration Script
# =============================================
# Configures database connection string using environment variables
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$Server = "localhost",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 1433,
    
    [Parameter(Mandatory=$false)]
    [string]$Database = "ITHelpDesk",
    
    [Parameter(Mandatory=$false)]
    [string]$Username = "ithelpdesk_user",
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseAppSettings
)

$ErrorActionPreference = "Stop"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Configuring Database Connection String" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "⚠️  Not running as Administrator. Environment variables will be set for current user only." -ForegroundColor Yellow
    $scope = "User"
} else {
    $scope = "Machine"
}

# Get connection details
if (-not $Password) {
    Write-Host "Enter database connection details:" -ForegroundColor Yellow
    Write-Host ""
    
    $Server = Read-Host "Server IP/Hostname [$Server]"
    if ([string]::IsNullOrWhiteSpace($Server)) { $Server = "localhost" }
    
    $PortInput = Read-Host "Port [$Port]"
    if (-not [string]::IsNullOrWhiteSpace($PortInput)) { $Port = [int]$PortInput }
    
    $Database = Read-Host "Database Name [$Database]"
    if ([string]::IsNullOrWhiteSpace($Database)) { $Database = "ITHelpDesk" }
    
    $Username = Read-Host "Username [$Username]"
    if ([string]::IsNullOrWhiteSpace($Username)) { $Username = "ithelpdesk_user" }
    
    $securePassword = Read-Host "Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

# Build connection string
$connectionString = "Server=$Server,$Port;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;Encrypt=True;"

Write-Host ""
Write-Host "Connection String:" -ForegroundColor Cyan
Write-Host "  Server: $Server,$Port" -ForegroundColor White
Write-Host "  Database: $Database" -ForegroundColor White
Write-Host "  Username: $Username" -ForegroundColor White
Write-Host ""

if ($UseAppSettings) {
    # Update appsettings.Production.json
    Write-Host "Updating appsettings.Production.json..." -ForegroundColor Yellow
    
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    $rootPath = Split-Path -Parent $scriptPath
    $appSettingsPath = Join-Path $rootPath "ITHelpDesk\appsettings.Production.json"
    
    if (-not (Test-Path $appSettingsPath)) {
        # Create from appsettings.json template
        $templatePath = Join-Path $rootPath "ITHelpDesk\appsettings.json"
        if (Test-Path $templatePath) {
            $template = Get-Content $templatePath -Raw | ConvertFrom-Json
        } else {
            $template = @{
                ConnectionStrings = @{
                    DefaultConnection = ""
                }
                Logging = @{
                    LogLevel = @{
                        Default = "Information"
                        Microsoft.AspNetCore = "Warning"
                    }
                }
                AllowedHosts = "*"
            } | ConvertTo-Json -Depth 10 | ConvertFrom-Json
        }
        
        $template.ConnectionStrings.DefaultConnection = $connectionString
        $template | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    } else {
        $json = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $json.ConnectionStrings.DefaultConnection = $connectionString
        $json | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    }
    
    Write-Host "✅ appsettings.Production.json updated" -ForegroundColor Green
} else {
    # Use Environment Variables (More Secure)
    Write-Host "Setting environment variable..." -ForegroundColor Yellow
    
    try {
        [System.Environment]::SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            $connectionString,
            $scope
        )
        
        Write-Host "✅ Environment variable set successfully" -ForegroundColor Green
        Write-Host "   Scope: $scope" -ForegroundColor Gray
        
        if ($scope -eq "Machine") {
            Write-Host "   Note: IIS application pool may need to be restarted" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ Failed to set environment variable: $_" -ForegroundColor Red
        exit 1
    }
}

# Test connection (optional)
Write-Host ""
$testConnection = Read-Host "Test database connection? (Y/N)"
if ($testConnection -eq "Y") {
    Write-Host "Testing connection..." -ForegroundColor Yellow
    
    try {
        # Try to use sqlcmd or .NET to test connection
        $testResult = sqlcmd -S "$Server,$Port" -U $Username -P $Password -d $Database -Q "SELECT 1" -b 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Connection test successful!" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Connection test failed. Please verify:" -ForegroundColor Yellow
            Write-Host "   - SQL Server is running" -ForegroundColor White
            Write-Host "   - Firewall allows port $Port" -ForegroundColor White
            Write-Host "   - Credentials are correct" -ForegroundColor White
        }
    } catch {
        Write-Host "⚠️  Could not test connection (sqlcmd not available)" -ForegroundColor Yellow
        Write-Host "   Please test manually" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "✅ Connection String Configuration Complete!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

if ($scope -eq "Machine") {
    Write-Host "Important: Restart IIS Application Pool to apply changes:" -ForegroundColor Yellow
    Write-Host "  Restart-WebAppPool -Name 'ITHelpDeskAppPool'" -ForegroundColor White
    Write-Host ""
}

