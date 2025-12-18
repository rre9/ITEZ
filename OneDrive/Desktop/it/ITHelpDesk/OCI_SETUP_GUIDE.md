# دليل إعداد SQL Server على OCI Windows Server

## 📋 المتطلبات الأساسية

### على السيرفر (OCI Windows Server):
1. ✅ SQL Server مثبت (Express/Standard/Enterprise)
2. ✅ SQL Server Authentication مفعل
3. ✅ Windows Firewall مفتوح للمنفذ 1433
4. ✅ Security List في OCI يسمح بالاتصال

---

## 🔧 الخطوة 1: إعداد SQL Server على OCI

### 1.1 تفعيل SQL Server Authentication:

1. افتح **SQL Server Management Studio (SSMS)**
2. اتصل بالسيرفر باستخدام Windows Authentication
3. انقر بالزر الأيمن على السيرفر → **Properties**
4. اذهب إلى **Security**
5. اختر **SQL Server and Windows Authentication mode**
6. اضغط **OK** وأعد تشغيل SQL Server Service

### 1.2 إنشاء قاعدة البيانات والمستخدم:

```sql
-- إنشاء قاعدة البيانات
CREATE DATABASE ITHelpDesk;
GO

-- إنشاء مستخدم SQL Server
CREATE LOGIN ithelpdesk_user WITH PASSWORD = 'YourStrongPassword123!';
GO

-- استخدام قاعدة البيانات
USE ITHelpDesk;
GO

-- إعطاء الصلاحيات للمستخدم
CREATE USER ithelpdesk_user FOR LOGIN ithelpdesk_user;
GO

-- إعطاء الصلاحيات الكاملة (أو حسب الحاجة)
ALTER ROLE db_owner ADD MEMBER ithelpdesk_user;
GO
```

### 1.3 تفعيل TCP/IP Protocol:

1. افتح **SQL Server Configuration Manager**
2. اذهب إلى **SQL Server Network Configuration** → **Protocols for [Instance Name]**
3. فعّل **TCP/IP**
4. انقر بالزر الأيمن على **TCP/IP** → **Properties**
5. في تبويب **IP Addresses**:
   - **IPAll** → **TCP Dynamic Ports**: اتركه فارغاً أو ضع `1433`
   - **IPAll** → **TCP Port**: `1433`
6. أعد تشغيل **SQL Server Service**

---

## 🔥 الخطوة 2: إعداد Windows Firewall

### 2.1 فتح المنفذ 1433:

#### طريقة 1: من PowerShell (كمسؤول):
```powershell
# فتح المنفذ 1433 للاتصالات الواردة
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

#### طريقة 2: من Windows Firewall GUI:
1. افتح **Windows Defender Firewall with Advanced Security**
2. **Inbound Rules** → **New Rule**
3. اختر **Port** → **Next**
4. اختر **TCP** واكتب `1433` → **Next**
5. اختر **Allow the connection** → **Next**
6. اختر جميع Profiles → **Next**
7. اكتب اسم: `SQL Server` → **Finish**

---

## 🌐 الخطوة 3: إعداد OCI Security List

### 3.1 فتح المنفذ في OCI:

1. اذهب إلى **OCI Console** → **Networking** → **Virtual Cloud Networks**
2. اختر VCN الخاص بك
3. اذهب إلى **Security Lists**
4. اختر Security List الخاص بالسيرفر
5. اضغط **Add Ingress Rules**
6. أدخل:
   - **Source Type**: CIDR
   - **Source CIDR**: `0.0.0.0/0` (للعام) أو IP محدد
   - **IP Protocol**: TCP
   - **Destination Port Range**: `1433`
   - **Description**: `SQL Server Access`
7. اضغط **Add Ingress Rules**

### 3.2 إعداد Network Security Group (اختياري):

إذا كنت تستخدم NSG:
1. اذهب إلى **Network Security Groups**
2. اختر NSG الخاص بك
3. اضغط **Add Ingress Rules**
4. نفس الإعدادات أعلاه

---

## 💻 الخطوة 4: تحديث Connection String

### 4.1 الحصول على IP أو Hostname:

#### من OCI Console:
1. اذهب إلى **Compute** → **Instances**
2. اختر السيرفر الخاص بك
3. انسخ **Public IP** أو **Private IP**

#### من السيرفر نفسه:
```powershell
# في PowerShell على السيرفر
ipconfig
# أو
hostname
```

### 4.2 تحديث appsettings.Production.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PUBLIC_IP_OR_HOSTNAME;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
  }
}
```

#### أمثلة:

**باستخدام Public IP:**
```json
"DefaultConnection": "Server=123.45.67.89;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

**باستخدام Private IP (إذا كان التطبيق على نفس VCN):**
```json
"DefaultConnection": "Server=10.0.1.5;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

**باستخدام Hostname:**
```json
"DefaultConnection": "Server=sqlserver.yourdomain.com;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=YourPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

---

## ✅ الخطوة 5: اختبار الاتصال

### 5.1 من جهازك المحلي:

#### باستخدام SSMS:
1. افتح **SQL Server Management Studio**
2. **Server name**: `YOUR_PUBLIC_IP` أو `YOUR_HOSTNAME`
3. **Authentication**: SQL Server Authentication
4. **Login**: `ithelpdesk_user`
5. **Password**: كلمة المرور
6. اضغط **Connect**

#### باستخدام PowerShell:
```powershell
# اختبار الاتصال
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 1433
```

#### باستخدام sqlcmd:
```powershell
sqlcmd -S YOUR_PUBLIC_IP -U ithelpdesk_user -P YourPassword123! -Q "SELECT @@VERSION"
```

### 5.2 من التطبيق:

```bash
# في مجلد المشروع
dotnet ef database update
```

---

## 🔒 الخطوة 6: الأمان (مهم جداً!)

### 6.1 استخدام SSL/TLS:

```json
"DefaultConnection": "Server=YOUR_IP;Database=ITHelpDesk;User Id=user;Password=pass;Encrypt=True;TrustServerCertificate=True;"
```

### 6.2 تقييد IP Addresses:

في OCI Security List، بدلاً من `0.0.0.0/0`، استخدم IP محدد:
- **Source CIDR**: `YOUR_OFFICE_IP/32`

### 6.3 استخدام Private IP:

إذا كان التطبيق على نفس VCN:
- استخدم **Private IP** بدلاً من Public IP
- لا تفتح المنفذ 1433 للعام في Security List

---

## 🚀 الخطوة 7: نشر التطبيق

### 7.1 تحديث Environment:

```bash
# في OCI Server
$env:ASPNETCORE_ENVIRONMENT="Production"
```

### 7.2 تشغيل Migrations:

```bash
# على السيرفر
dotnet ef database update
```

### 7.3 تشغيل التطبيق:

```bash
dotnet run
# أو
dotnet publish -c Release
```

---

## 📝 ملاحظات مهمة

1. **TrustServerCertificate=True**: استخدمه فقط للتطوير. للإنتاج، استخدم شهادة SSL صحيحة
2. **Connection Timeout**: زد القيمة إذا كان الاتصال بطيئاً
3. **MultipleActiveResultSets**: مفيد لـ Entity Framework
4. **Backup**: تأكد من إعداد نسخ احتياطي يومي
5. **Monitoring**: راقب استخدام الموارد والأداء

---

## 🐛 حل المشاكل الشائعة

### المشكلة: "Cannot connect to server"

**الحلول:**
1. ✅ تأكد من أن SQL Server Service يعمل
2. ✅ تأكد من فتح المنفذ 1433 في Windows Firewall
3. ✅ تأكد من فتح المنفذ في OCI Security List
4. ✅ تأكد من تفعيل TCP/IP في SQL Server Configuration Manager
5. ✅ تأكد من صحة IP Address

### المشكلة: "Login failed for user"

**الحلول:**
1. ✅ تأكد من تفعيل SQL Server Authentication
2. ✅ تأكد من صحة اسم المستخدم وكلمة المرور
3. ✅ تأكد من إعطاء الصلاحيات للمستخدم

### المشكلة: "A network-related or instance-specific error"

**الحلول:**
1. ✅ تأكد من أن SQL Server Browser Service يعمل (إذا كان هناك Named Instance)
2. ✅ جرب الاتصال من داخل السيرفر أولاً
3. ✅ تحقق من SQL Server Error Log

---

## 📞 الدعم

إذا واجهت مشاكل:
1. راجع **SQL Server Error Log**
2. راجع **Windows Event Viewer**
3. اختبر الاتصال من داخل السيرفر أولاً
4. تأكد من جميع الخطوات أعلاه

---

**آخر تحديث**: 2024

