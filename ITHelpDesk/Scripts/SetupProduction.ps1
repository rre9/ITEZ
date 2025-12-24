# =============================================
# Script لإعداد التطبيق على OCI Server
# =============================================
# استخدم هذا الملف بعد رفع الكود على السيرفر
# =============================================

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "إعداد IT Help Desk على OCI Server" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# الحصول على معلومات الاتصال
Write-Host "أدخل معلومات قاعدة البيانات:" -ForegroundColor Yellow
Write-Host ""

$serverIP = Read-Host "IP أو Hostname للسيرفر (مثال: 123.45.67.89)"
$dbName = Read-Host "اسم قاعدة البيانات (افتراضي: ITHelpDesk)"
if ([string]::IsNullOrWhiteSpace($dbName)) { $dbName = "ITHelpDesk" }

$dbUser = Read-Host "اسم المستخدم (مثال: ithelpdesk_user)"
$dbPassword = Read-Host "كلمة المرور" -AsSecureString
$dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword))

Write-Host ""
Write-Host "اختر طريقة الإعداد:" -ForegroundColor Yellow
Write-Host "1. تحديث appsettings.Production.json (أسهل)" -ForegroundColor White
Write-Host "2. استخدام Environment Variables (أكثر أماناً)" -ForegroundColor White
Write-Host ""

$choice = Read-Host "اختر (1 أو 2)"

# بناء Connection String
$connectionString = "Server=$serverIP;Database=$dbName;User Id=$dbUser;Password=$dbPasswordPlain;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"

if ($choice -eq "1") {
    # تحديث appsettings.Production.json
    Write-Host ""
    Write-Host "تحديث appsettings.Production.json..." -ForegroundColor Cyan
    
    $appsettingsPath = "appsettings.Production.json"
    
    if (Test-Path $appsettingsPath) {
        $json = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $json.ConnectionStrings.DefaultConnection = $connectionString
        
        $json | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
        Write-Host "✅ تم تحديث appsettings.Production.json بنجاح!" -ForegroundColor Green
    } else {
        Write-Host "❌ ملف appsettings.Production.json غير موجود!" -ForegroundColor Red
    }
}
elseif ($choice -eq "2") {
    # استخدام Environment Variables
    Write-Host ""
    Write-Host "إعداد Environment Variables..." -ForegroundColor Cyan
    
    [System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $connectionString, "Machine")
    
    Write-Host "✅ تم إعداد Environment Variable بنجاح!" -ForegroundColor Green
    Write-Host ""
    Write-Host "ملاحظة: يجب إعادة تشغيل التطبيق أو IIS لتطبيق التغييرات" -ForegroundColor Yellow
}
else {
    Write-Host "❌ اختيار غير صحيح!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "الخطوات التالية:" -ForegroundColor Cyan
Write-Host "1. تأكد من أن SQL Server يعمل ويمكن الوصول إليه" -ForegroundColor White
Write-Host "2. شغّل: dotnet ef database update" -ForegroundColor White
Write-Host "3. شغّل التطبيق" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# تنظيف كلمة المرور من الذاكرة
$dbPasswordPlain = $null
$dbPassword = $null

pause

