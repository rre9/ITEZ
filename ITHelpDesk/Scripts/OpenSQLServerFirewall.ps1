# =============================================
# Script لفتح منفذ SQL Server (1433) في Windows Firewall
# =============================================
# يجب تشغيل هذا الملف كمسؤول (Run as Administrator)
# =============================================

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "فتح منفذ SQL Server (1433) في Windows Firewall" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# التحقق من الصلاحيات
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "❌ خطأ: يجب تشغيل هذا الملف كمسؤول!" -ForegroundColor Red
    Write-Host "انقر بالزر الأيمن واختر 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit 1
}

# التحقق من وجود القاعدة
$existingRule = Get-NetFirewallRule -DisplayName "SQL Server" -ErrorAction SilentlyContinue

if ($existingRule) {
    Write-Host "⚠️  القاعدة موجودة بالفعل. هل تريد حذفها وإنشاء واحدة جديدة؟" -ForegroundColor Yellow
    $response = Read-Host "اكتب 'y' للموافقة أو 'n' للإلغاء"
    
    if ($response -eq 'y' -or $response -eq 'Y') {
        Remove-NetFirewallRule -DisplayName "SQL Server"
        Write-Host "✅ تم حذف القاعدة القديمة" -ForegroundColor Green
    } else {
        Write-Host "تم الإلغاء." -ForegroundColor Yellow
        pause
        exit 0
    }
}

# إنشاء القاعدة
try {
    New-NetFirewallRule `
        -DisplayName "SQL Server" `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort 1433 `
        -Action Allow `
        -Description "Allow SQL Server connections on port 1433" `
        -Profile Domain, Private, Public
    
    Write-Host ""
    Write-Host "✅ تم فتح المنفذ 1433 بنجاح!" -ForegroundColor Green
    Write-Host ""
    Write-Host "الخطوات التالية:" -ForegroundColor Cyan
    Write-Host "1. تأكد من تفعيل TCP/IP في SQL Server Configuration Manager" -ForegroundColor White
    Write-Host "2. أعد تشغيل SQL Server Service" -ForegroundColor White
    Write-Host "3. افتح المنفذ 1433 في OCI Security List" -ForegroundColor White
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "❌ خطأ في إنشاء القاعدة:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
}

Write-Host "=============================================" -ForegroundColor Cyan
pause

