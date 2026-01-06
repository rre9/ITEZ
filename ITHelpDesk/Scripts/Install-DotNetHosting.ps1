# =============================================
# .NET 8.0 Hosting Bundle Installation Script
# =============================================
# Downloads and installs .NET 8.0 Hosting Bundle
# =============================================

$ErrorActionPreference = "Stop"

Write-Host "Installing .NET 8.0 Hosting Bundle..." -ForegroundColor Cyan

# Check if .NET 8.0 is already installed
$dotnetInstalled = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnetInstalled) {
    $dotnetVersion = dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App 8.0"
    if ($dotnetVersion) {
        Write-Host "✅ .NET 8.0 Hosting Bundle is already installed" -ForegroundColor Green
        Write-Host "   Installed version: $dotnetVersion" -ForegroundColor Gray
        return
    }
}

# Download URL for .NET 8.0 Hosting Bundle
$downloadUrl = "https://download.visualstudio.microsoft.com/download/pr/9c0d0c0e-3e3e-4e3e-8e3e-3e3e3e3e3e3e/dotnet-hosting-8.0.0-win.exe"
$downloadPath = "$env:TEMP\dotnet-hosting-8.0.0-win.exe"

# Get the latest download URL
Write-Host "Fetching latest .NET 8.0 Hosting Bundle download URL..." -ForegroundColor Yellow
try {
    # Use the official download page to get the latest version
    $downloadUrl = "https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-aspnetcore-8.0.0-windows-hosting-bundle-installer"
    
    # Direct download link (update this if needed)
    $downloadUrl = "https://download.visualstudio.microsoft.com/download/pr/9c0d0c0e-3e3e-4e3e-8e3e-3e3e3e3e3e3e/dotnet-hosting-8.0.0-win.exe"
    
    Write-Host "Downloading .NET 8.0 Hosting Bundle..." -ForegroundColor Yellow
    Write-Host "URL: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Gray
    
    # Alternative: Download from official page
    $response = Invoke-WebRequest -Uri "https://dotnet.microsoft.com/download/dotnet/8.0" -UseBasicParsing
    $downloadLink = $response.Links | Where-Object { $_.href -like "*hosting*" -and $_.href -like "*.exe" } | Select-Object -First 1
    
    if (-not $downloadLink) {
        Write-Host "⚠️  Could not auto-detect download URL. Please download manually:" -ForegroundColor Yellow
        Write-Host "   https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
        Write-Host "   Look for 'Hosting Bundle' download" -ForegroundColor White
        Write-Host ""
        $manualDownload = Read-Host "Press Enter after downloading and saving to $downloadPath, or 'S' to skip"
        if ($manualDownload -eq 'S') { return }
    } else {
        $downloadUrl = $downloadLink.href
    }
} catch {
    Write-Host "⚠️  Could not fetch download URL automatically" -ForegroundColor Yellow
    Write-Host "Please download manually from:" -ForegroundColor White
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host "Save as: $downloadPath" -ForegroundColor White
    Write-Host ""
    $manualDownload = Read-Host "Press Enter after downloading, or 'S' to skip"
    if ($manualDownload -eq 'S') { return }
}

# Download the installer
if (-not (Test-Path $downloadPath)) {
    Write-Host "Downloading .NET 8.0 Hosting Bundle..." -ForegroundColor Yellow
    try {
        # Try direct download
        Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadPath -UseBasicParsing
        Write-Host "✅ Download complete" -ForegroundColor Green
    } catch {
        Write-Host "❌ Download failed: $_" -ForegroundColor Red
        Write-Host "Please download manually and save to: $downloadPath" -ForegroundColor Yellow
        Read-Host "Press Enter after downloading"
    }
}

# Install .NET 8.0 Hosting Bundle
if (Test-Path $downloadPath) {
    Write-Host "Installing .NET 8.0 Hosting Bundle..." -ForegroundColor Yellow
    Write-Host "This may take a few minutes..." -ForegroundColor Gray
    
    try {
        $process = Start-Process -FilePath $downloadPath -ArgumentList "/quiet", "/norestart" -Wait -PassThru
        
        if ($process.ExitCode -eq 0 -or $process.ExitCode -eq 3010) {
            Write-Host "✅ .NET 8.0 Hosting Bundle installed successfully" -ForegroundColor Green
            
            # Verify installation
            Start-Sleep -Seconds 3
            $dotnetVersion = dotnet --list-runtimes 2>$null | Select-String "Microsoft.AspNetCore.App 8.0"
            if ($dotnetVersion) {
                Write-Host "   Verified: $dotnetVersion" -ForegroundColor Gray
            }
        } else {
            Write-Host "⚠️  Installation completed with exit code: $($process.ExitCode)" -ForegroundColor Yellow
            Write-Host "   (Exit code 3010 means restart required)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ Installation failed: $_" -ForegroundColor Red
        exit 1
    }
    
    # Clean up
    Remove-Item $downloadPath -ErrorAction SilentlyContinue
} else {
    Write-Host "❌ Installer not found at: $downloadPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ .NET 8.0 Hosting Bundle setup complete!" -ForegroundColor Green

