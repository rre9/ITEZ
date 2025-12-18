# دليل نشر التطبيق على OCI Server

## 📋 الخطوات السريعة

### ✅ الخطوة 1: رفع الكود (الآن)

**لا تحتاج تغيير أي شيء!** 

الكود جاهز للرفع:
- ✅ `appsettings.Production.json` موجود (مستبعد من Git)
- ✅ الكود يعمل مع LocalDB للتطوير
- ✅ الكود جاهز للإنتاج

**ارفع الكود الآن كما هو!**

---

### 🔧 الخطوة 2: على السيرفر (بعد الرفع)

#### أ. إعداد SQL Server:
1. شغّل `Scripts/CreateDatabase_OCI.sql` في SSMS
2. فعّل TCP/IP وافتح Firewall (شغّل `Scripts/OpenSQLServerFirewall.ps1`)
3. افتح المنفذ 1433 في OCI Security List

#### ب. إعداد Connection String:

**الطريقة 1: تحديث الملف مباشرة (أسهل)**

عدّل `appsettings.Production.json` على السيرفر:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_IP;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
  }
}
```

**الطريقة 2: استخدام Script (موصى به)**

شغّل `Scripts/SetupProduction.ps1` على السيرفر:
```powershell
.\Scripts\SetupProduction.ps1
```

**الطريقة 3: Environment Variables (أكثر أماناً)**

```powershell
# في PowerShell (كمسؤول)
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=IP;Database=ITHelpDesk;User Id=user;Password=pass;...", "Machine")
```

---

### 🚀 الخطوة 3: تشغيل التطبيق

```powershell
# 1. تحديث قاعدة البيانات
dotnet ef database update

# 2. تشغيل التطبيق
dotnet run --environment Production

# أو للنشر
dotnet publish -c Release
```

---

## 📝 ملخص

| الخطوة | متى؟ | ماذا تفعل؟ |
|--------|------|------------|
| **رفع الكود** | الآن | ارفع الكود كما هو (لا تغير شي) |
| **إعداد SQL Server** | على السيرفر | شغّل SQL Scripts |
| **تحديث Connection String** | على السيرفر | عدّل `appsettings.Production.json` أو استخدم Environment Variables |
| **تشغيل Migrations** | على السيرفر | `dotnet ef database update` |
| **تشغيل التطبيق** | على السيرفر | `dotnet run` أو نشر |

---

## 🔒 الأمان

- ✅ `appsettings.Production.json` **مستبعد من Git** (لن يُرفع)
- ✅ يمكنك وضع معلومات حقيقية في الملف على السيرفر
- ✅ أو استخدم Environment Variables (أكثر أماناً)

---

## ❓ أسئلة شائعة

**س: هل أغير الكود الآن؟**
ج: **لا!** ارفع الكود كما هو، ثم غيّر على السيرفر.

**س: أين أضع Connection String؟**
ج: في `appsettings.Production.json` على السيرفر (بعد الرفع).

**س: هل الملف آمن؟**
ج: نعم، الملف مستبعد من Git ولن يُرفع.

---

**آخر تحديث**: 2024

