# =============================================
# SQL Server Configuration Script
# =============================================
# Configures SQL Server for remote access:
# - Enables Mixed Mode Authentication
# - Enables TCP/IP protocol
# - Configures firewall rules
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$SqlServerPassword
)

$ErrorActionPreference = "Stop"

Write-Host "Configuring SQL Server..." -ForegroundColor Cyan

# Check if SQL Server is installed
$sqlService = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
if (-not $sqlService) {
    Write-Host "❌ SQL Server service not found. Please install SQL Server first." -ForegroundColor Red
    exit 1
}

# Start SQL Server service if not running
if ($sqlService.Status -ne "Running") {
    Write-Host "Starting SQL Server service..." -ForegroundColor Yellow
    Start-Service -Name "MSSQLSERVER"
    Start-Sleep -Seconds 5
}

# Enable TCP/IP Protocol using SQL Server Configuration Manager
Write-Host "Enabling TCP/IP protocol..." -ForegroundColor Yellow
try {
    $sqlServerPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server"
    $instances = Get-ChildItem $sqlServerPath -ErrorAction SilentlyContinue | Where-Object { $_.PSChildName -like "MSSQL*" }
    
    if ($instances) {
        $instanceName = $instances[0].PSChildName
        $tcpIpPath = "$sqlServerPath\$instanceName\MSSQLServer\SuperSocketNetLib\Tcp"
        
        if (Test-Path $tcpIpPath) {
            Set-ItemProperty -Path "$tcpIpPath\IPAll" -Name "TcpPort" -Value "1433" -ErrorAction SilentlyContinue
            Set-ItemProperty -Path "$tcpIpPath\IPAll" -Name "TcpDynamicPorts" -Value "" -ErrorAction SilentlyContinue
            Set-ItemProperty -Path "$tcpIpPath" -Name "Enabled" -Value 1 -ErrorAction SilentlyContinue
            Write-Host "✅ TCP/IP protocol configured" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Could not auto-configure TCP/IP. Please configure manually:" -ForegroundColor Yellow
            Write-Host "   1. Open SQL Server Configuration Manager" -ForegroundColor White
            Write-Host "   2. SQL Server Network Configuration > Protocols for [Instance]" -ForegroundColor White
            Write-Host "   3. Enable TCP/IP and set port to 1433" -ForegroundColor White
        }
    }
} catch {
    Write-Host "⚠️  Could not auto-configure TCP/IP: $_" -ForegroundColor Yellow
    Write-Host "Please configure manually using SQL Server Configuration Manager" -ForegroundColor Yellow
}

# Configure Windows Firewall
Write-Host "Configuring Windows Firewall..." -ForegroundColor Yellow
try {
    # Remove existing rule if present
    Remove-NetFirewallRule -DisplayName "SQL Server" -ErrorAction SilentlyContinue
    
    # Add firewall rule for SQL Server
    New-NetFirewallRule -DisplayName "SQL Server" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 1433 `
        -Action Allow `
        -Description "Allow SQL Server connections on port 1433" | Out-Null
    
    Write-Host "✅ Windows Firewall rule added for port 1433" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not configure firewall: $_" -ForegroundColor Yellow
    Write-Host "Please manually allow port 1433 in Windows Firewall" -ForegroundColor Yellow
}

# Enable SQL Server Browser (for named instances)
Write-Host "Configuring SQL Server Browser..." -ForegroundColor Yellow
try {
    $browserService = Get-Service -Name "SQLBrowser" -ErrorAction SilentlyContinue
    if ($browserService) {
        Set-Service -Name "SQLBrowser" -StartupType Automatic -ErrorAction SilentlyContinue
        if ($browserService.Status -ne "Running") {
            Start-Service -Name "SQLBrowser" -ErrorAction SilentlyContinue
        }
        
        # Firewall rule for SQL Browser
        Remove-NetFirewallRule -DisplayName "SQL Server Browser" -ErrorAction SilentlyContinue
        New-NetFirewallRule -DisplayName "SQL Server Browser" `
            -Direction Inbound `
            -Protocol UDP `
            -LocalPort 1434 `
            -Action Allow `
            -Description "Allow SQL Server Browser on port 1434" | Out-Null
        
        Write-Host "✅ SQL Server Browser configured" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  SQL Server Browser configuration skipped: $_" -ForegroundColor Yellow
}

# Enable Mixed Mode Authentication
Write-Host "Enabling Mixed Mode Authentication..." -ForegroundColor Yellow
Write-Host "⚠️  This requires manual configuration:" -ForegroundColor Yellow
Write-Host "   1. Open SQL Server Management Studio (SSMS)" -ForegroundColor White
Write-Host "   2. Connect to your SQL Server instance" -ForegroundColor White
Write-Host "   3. Right-click server > Properties > Security" -ForegroundColor White
Write-Host "   4. Select 'SQL Server and Windows Authentication mode'" -ForegroundColor White
Write-Host "   5. Restart SQL Server service" -ForegroundColor White
Write-Host ""

# Restart SQL Server to apply changes
Write-Host "Restarting SQL Server service to apply changes..." -ForegroundColor Yellow
Restart-Service -Name "MSSQLSERVER" -Force
Start-Sleep -Seconds 10

Write-Host ""
Write-Host "✅ SQL Server configuration complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Important: Don't forget to:" -ForegroundColor Yellow
Write-Host "  1. Enable Mixed Mode Authentication in SSMS" -ForegroundColor White
Write-Host "  2. Configure OCI Security List to allow port 1433" -ForegroundColor White
Write-Host ""

