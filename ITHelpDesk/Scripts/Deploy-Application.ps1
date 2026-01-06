# =============================================
# Application Deployment Script
# =============================================
# Publishes and deploys the ASP.NET Core application to IIS
# =============================================

param(
    [Parameter(Mandatory=$false)]
    [string]$PublishPath = "C:\inetpub\wwwroot\ITHelpDesk",
    
    [Parameter(Mandatory=$false)]
    [string]$SiteName = "ITHelpDesk",
    
    [Parameter(Mandatory=$false)]
    [string]$AppPoolName = "ITHelpDeskAppPool",
    
    [Parameter(Mandatory=$false)]
    [int]$Port = 80,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$projectPath = Join-Path $rootPath "ITHelpDesk"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Deploying IT Help Desk Application" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check if project exists
if (-not (Test-Path $projectPath)) {
    Write-Host "❌ Project not found at: $projectPath" -ForegroundColor Red
    Write-Host "Please ensure you're running this script from the correct location" -ForegroundColor Yellow
    exit 1
}

# Step 1: Publish the application
if (-not $SkipPublish) {
    Write-Host "Step 1: Publishing application..." -ForegroundColor Yellow
    
    $publishProfile = Join-Path $env:TEMP "ITHelpDesk_Publish"
    
    if (Test-Path $publishProfile) {
        Remove-Item $publishProfile -Recurse -Force
    }
    
    Write-Host "  Publishing to: $publishProfile" -ForegroundColor Gray
    Push-Location $projectPath
    
    try {
        dotnet publish -c Release -o $publishProfile
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Publish failed" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "✅ Application published successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Publish error: $_" -ForegroundColor Red
        exit 1
    } finally {
        Pop-Location
    }
} else {
    $publishProfile = $PublishPath
    if (-not (Test-Path $publishProfile)) {
        Write-Host "❌ Publish path not found: $publishProfile" -ForegroundColor Red
        exit 1
    }
}

# Step 2: Create IIS Application Pool
Write-Host ""
Write-Host "Step 2: Configuring IIS Application Pool..." -ForegroundColor Yellow

$appPool = Get-IISAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
if (-not $appPool) {
    Write-Host "  Creating application pool: $AppPoolName" -ForegroundColor Gray
    New-WebAppPool -Name $AppPoolName | Out-Null
    $appPool = Get-IISAppPool -Name $AppPoolName
}

# Configure application pool
Write-Host "  Configuring application pool..." -ForegroundColor Gray
$appPool.managedRuntimeVersion = ""  # No managed runtime (for .NET Core)
$appPool.managedPipelineMode = "Integrated"
$appPool.startMode = "AlwaysRunning"
$appPool.processModel.idleTimeout = [TimeSpan]::FromMinutes(0)
$appPool | Set-Item

Write-Host "✅ Application pool configured" -ForegroundColor Green

# Step 3: Create/Update IIS Website
Write-Host ""
Write-Host "Step 3: Creating IIS Website..." -ForegroundColor Yellow

$website = Get-Website -Name $SiteName -ErrorAction SilentlyContinue

if ($website) {
    Write-Host "  Website already exists. Updating..." -ForegroundColor Gray
    Set-Website -Name $SiteName -PhysicalPath $PublishPath -ApplicationPool $AppPoolName
} else {
    Write-Host "  Creating website: $SiteName" -ForegroundColor Gray
    
    # Check if port is available
    $existingSite = Get-Website | Where-Object { $_.bindings.Collection.bindingInformation -like "*:$Port:*" }
    if ($existingSite) {
        Write-Host "⚠️  Port $Port is already in use by: $($existingSite.Name)" -ForegroundColor Yellow
        $usePort = Read-Host "Continue anyway? (Y/N)"
        if ($usePort -ne "Y") {
            exit 1
        }
    }
    
    New-Website -Name $SiteName -PhysicalPath $PublishPath -Port $Port -ApplicationPool $AppPoolName
}

Write-Host "✅ Website created/updated" -ForegroundColor Green

# Step 4: Copy published files
Write-Host ""
Write-Host "Step 4: Copying application files..." -ForegroundColor Yellow

if (-not (Test-Path $PublishPath)) {
    New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
}

Write-Host "  Copying from: $publishProfile" -ForegroundColor Gray
Write-Host "  Copying to: $PublishPath" -ForegroundColor Gray

# Stop website during deployment
if ($website) {
    Stop-Website -Name $SiteName -ErrorAction SilentlyContinue
}

try {
    # Copy files
    Copy-Item -Path "$publishProfile\*" -Destination $PublishPath -Recurse -Force
    
    Write-Host "✅ Files copied successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error copying files: $_" -ForegroundColor Red
    exit 1
} finally {
    # Start website
    Start-Website -Name $SiteName -ErrorAction SilentlyContinue
}

# Step 5: Set permissions
Write-Host ""
Write-Host "Step 5: Setting file permissions..." -ForegroundColor Yellow

$iisUser = "IIS AppPool\$AppPoolName"
try {
    $acl = Get-Acl $PublishPath
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($iisUser, "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl $PublishPath $acl
    
    Write-Host "✅ Permissions set for: $iisUser" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not set permissions: $_" -ForegroundColor Yellow
    Write-Host "   You may need to set permissions manually" -ForegroundColor Yellow
}

# Step 6: Create web.config if needed (for IIS)
Write-Host ""
Write-Host "Step 6: Configuring web.config..." -ForegroundColor Yellow

$webConfigPath = Join-Path $PublishPath "web.config"
if (-not (Test-Path $webConfigPath)) {
    $webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\ITHelpDesk.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
"@
    $webConfigContent | Set-Content $webConfigPath
    Write-Host "✅ web.config created" -ForegroundColor Green
} else {
    Write-Host "✅ web.config already exists" -ForegroundColor Green
}

# Clean up temporary publish folder
if (-not $SkipPublish -and (Test-Path $publishProfile)) {
    Remove-Item $publishProfile -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "✅ Application Deployment Complete!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Website Information:" -ForegroundColor Cyan
Write-Host "  Site Name: $SiteName" -ForegroundColor White
Write-Host "  URL: http://localhost:$Port" -ForegroundColor White
Write-Host "  Physical Path: $PublishPath" -ForegroundColor White
Write-Host "  Application Pool: $AppPoolName" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Configure connection string using: Configure-ConnectionString.ps1" -ForegroundColor White
Write-Host "2. Test the application at: http://localhost:$Port" -ForegroundColor White
Write-Host "3. Configure OCI Security List to allow HTTP/HTTPS traffic" -ForegroundColor White
Write-Host ""

