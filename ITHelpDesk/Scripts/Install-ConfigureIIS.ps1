# =============================================
# IIS Installation and Configuration Script
# =============================================
# Installs IIS with required features for ASP.NET Core
# =============================================

$ErrorActionPreference = "Stop"

Write-Host "Installing and configuring IIS..." -ForegroundColor Cyan

# Check if IIS is already installed
$iisFeature = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -ErrorAction SilentlyContinue
if ($iisFeature -and $iisFeature.State -eq "Enabled") {
    Write-Host "✅ IIS is already installed" -ForegroundColor Green
} else {
    Write-Host "Installing IIS with required features..." -ForegroundColor Yellow
    Write-Host "This may take several minutes..." -ForegroundColor Gray
    
    # Install IIS and required features
    $features = @(
        "IIS-WebServerRole",
        "IIS-WebServer",
        "IIS-CommonHttpFeatures",
        "IIS-HttpErrors",
        "IIS-ApplicationInit",
        "IIS-NetFxExtensibility45",
        "IIS-HealthAndDiagnostics",
        "IIS-HttpLogging",
        "IIS-Security",
        "IIS-RequestFiltering",
        "IIS-Performance",
        "IIS-HttpCompressionStatic",
        "IIS-WebSockets",
        "IIS-ApplicationDevelopment",
        "IIS-ASPNET45"
    )
    
    foreach ($feature in $features) {
        Write-Host "  Installing $feature..." -ForegroundColor Gray
        Enable-WindowsOptionalFeature -Online -FeatureName $feature -NoRestart -All | Out-Null
    }
    
    Write-Host "✅ IIS installed successfully" -ForegroundColor Green
}

# Verify IIS is running
$iisService = Get-Service -Name "W3SVC" -ErrorAction SilentlyContinue
if ($iisService) {
    if ($iisService.Status -ne "Running") {
        Write-Host "Starting IIS service..." -ForegroundColor Yellow
        Start-Service -Name "W3SVC"
    }
    Write-Host "✅ IIS service is running" -ForegroundColor Green
}

# Configure IIS for ASP.NET Core
Write-Host "Configuring IIS for ASP.NET Core..." -ForegroundColor Yellow

# Create application pool (will be used during deployment)
$appPoolName = "ITHelpDeskAppPool"
$appPool = Get-IISAppPool -Name $appPoolName -ErrorAction SilentlyContinue

if (-not $appPool) {
    Write-Host "  Creating application pool: $appPoolName" -ForegroundColor Gray
    New-WebAppPool -Name $appPoolName | Out-Null
    
    # Configure application pool
    $appPool = Get-IISAppPool -Name $appPoolName
    $appPool.managedRuntimeVersion = ""  # No managed runtime (for .NET Core)
    $appPool.managedPipelineMode = "Integrated"
    $appPool.startMode = "AlwaysRunning"
    $appPool.processModel.idleTimeout = [TimeSpan]::FromMinutes(0)  # Keep alive
    $appPool | Set-Item
}

Write-Host "✅ IIS configuration complete!" -ForegroundColor Green
Write-Host ""
Write-Host "IIS Application Pool created: $appPoolName" -ForegroundColor Gray
Write-Host ""

