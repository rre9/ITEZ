# =============================================
# Database Setup Script
# =============================================
# Creates database and user using CreateDatabase_OCI.sql
# =============================================

param(
    [Parameter(Mandatory=$true)]
    [string]$DatabaseUserPassword
)

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Setting up database..." -ForegroundColor Cyan

# Check if SQL Server is running
$sqlService = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
if (-not $sqlService -or $sqlService.Status -ne "Running") {
    Write-Host "❌ SQL Server service is not running" -ForegroundColor Red
    exit 1
}

# Get SQL Server instance name
$instanceName = "localhost"
$sqlServerPath = "HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server"
$instances = Get-ChildItem $sqlServerPath -ErrorAction SilentlyContinue | Where-Object { $_.PSChildName -like "MSSQL*" }

if ($instances) {
    $instanceName = $instances[0].PSChildName
}

# Check if sqlcmd is available
$sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
if (-not $sqlcmdPath) {
    Write-Host "⚠️  sqlcmd not found. Attempting to use SQL Server tools..." -ForegroundColor Yellow
    
    # Try to find sqlcmd in common locations
    $possiblePaths = @(
        "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\sqlcmd.exe",
        "C:\Program Files\Microsoft SQL Server\140\Tools\Binn\sqlcmd.exe"
    )
    
    $found = $false
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $env:Path += ";$(Split-Path -Parent $path)"
            $found = $true
            break
        }
    }
    
    if (-not $found) {
        Write-Host "❌ sqlcmd not found. Please install SQL Server Management Tools or run the SQL script manually in SSMS" -ForegroundColor Red
        Write-Host ""
        Write-Host "Script location: $scriptPath\CreateDatabase_OCI.sql" -ForegroundColor Yellow
        Write-Host "Please update the password in the script and run it in SSMS" -ForegroundColor Yellow
        exit 1
    }
}

# Read and modify the SQL script
$sqlScriptPath = Join-Path $scriptPath "CreateDatabase_OCI.sql"
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "❌ SQL script not found: $sqlScriptPath" -ForegroundColor Red
    exit 1
}

$sqlScript = Get-Content $sqlScriptPath -Raw

# Replace the password placeholder
$sqlScript = $sqlScript -replace "ChangeThisPassword123!", $DatabaseUserPassword

# Create temporary SQL file
$tempSqlFile = Join-Path $env:TEMP "CreateDatabase_OCI_Temp.sql"
$sqlScript | Set-Content $tempSqlFile

Write-Host "Creating database and user..." -ForegroundColor Yellow

# Try to connect and run the script
try {
    # First, try with Windows Authentication
    $result = sqlcmd -S $instanceName -E -i $tempSqlFile -b 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database setup completed successfully!" -ForegroundColor Green
    } else {
        # If Windows Auth fails, try with sa account
        Write-Host "⚠️  Windows Authentication failed. Trying with SQL Server Authentication..." -ForegroundColor Yellow
        Write-Host "Please enter 'sa' password:" -ForegroundColor Yellow
        $saPassword = Read-Host "SA Password" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($saPassword)
        $saPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        
        $result = sqlcmd -S $instanceName -U sa -P $saPasswordPlain -i $tempSqlFile -b 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database setup completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "❌ Database setup failed" -ForegroundColor Red
            Write-Host $result -ForegroundColor Red
            Write-Host ""
            Write-Host "Please run the SQL script manually in SSMS:" -ForegroundColor Yellow
            Write-Host "  $sqlScriptPath" -ForegroundColor White
            exit 1
        }
    }
} catch {
    Write-Host "❌ Error running SQL script: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please run the SQL script manually in SSMS:" -ForegroundColor Yellow
    Write-Host "  $sqlScriptPath" -ForegroundColor White
    Write-Host "  Remember to update the password in the script!" -ForegroundColor Yellow
    exit 1
} finally {
    # Clean up temporary file
    Remove-Item $tempSqlFile -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "Database Information:" -ForegroundColor Cyan
Write-Host "  Database Name: ITHelpDesk" -ForegroundColor White
Write-Host "  Username: ithelpdesk_user" -ForegroundColor White
Write-Host "  Password: [As provided]" -ForegroundColor White
Write-Host ""

