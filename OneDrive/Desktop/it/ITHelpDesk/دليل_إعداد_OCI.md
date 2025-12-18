# دليل إعداد SQL Server على OCI Windows Server

## 🎯 الخطوات السريعة

### 1️⃣ على السيرفر (OCI Windows):

#### أ. تفعيل SQL Server Authentication:
```
1. افتح SQL Server Management Studio (SSMS)
2. اتصل بالسيرفر → Properties → Security
3. اختر "SQL Server and Windows Authentication mode"
4. أعد تشغيل SQL Server Service
```

#### ب. إنشاء قاعدة البيانات والمستخدم:
```sql
CREATE DATABASE ITHelpDesk;
GO

CREATE LOGIN ithelpdesk_user WITH PASSWORD = 'كلمة_مرور_قوية_123!';
GO

USE ITHelpDesk;
GO

CREATE USER ithelpdesk_user FOR LOGIN ithelpdesk_user;
GO

ALTER ROLE db_owner ADD MEMBER ithelpdesk_user;
GO
```

#### ج. تفعيل TCP/IP:
```
1. افتح SQL Server Configuration Manager
2. SQL Server Network Configuration → Protocols
3. فعّل TCP/IP
4. Properties → IP Addresses → IPAll → TCP Port: 1433
5. أعد تشغيل SQL Server Service
```

#### د. فتح Firewall:
```powershell
# في PowerShell (كمسؤول)
New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

---

### 2️⃣ في OCI Console:

#### أ. فتح المنفذ 1433:
```
1. OCI Console → Networking → Virtual Cloud Networks
2. اختر VCN → Security Lists
3. Add Ingress Rules:
   - Source CIDR: 0.0.0.0/0 (أو IP محدد)
   - IP Protocol: TCP
   - Destination Port: 1433
   - Description: SQL Server Access
```

---

### 3️⃣ في المشروع:

#### أ. احصل على IP السيرفر:
- من OCI Console → Compute → Instances → Public IP

#### ب. عدّل `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PUBLIC_IP;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=كلمة_المرور;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;"
  }
}
```

**مثال:**
```json
"DefaultConnection": "Server=123.45.67.89;Database=ITHelpDesk;User Id=ithelpdesk_user;Password=MyPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
```

---

### 4️⃣ اختبار الاتصال:

#### من SSMS:
```
Server name: YOUR_PUBLIC_IP
Authentication: SQL Server Authentication
Login: ithelpdesk_user
Password: كلمة_المرور
```

#### من PowerShell:
```powershell
Test-NetConnection -ComputerName YOUR_PUBLIC_IP -Port 1433
```

---

## 🔒 نصائح الأمان:

1. ✅ **لا تفتح المنفذ للعام**: استخدم IP محدد في Security List
2. ✅ **استخدم كلمة مرور قوية**: 12+ حرف، أرقام، رموز
3. ✅ **استخدم Private IP**: إذا كان التطبيق على نفس VCN
4. ✅ **فعّل SSL**: للإنتاج (Encrypt=True)

---

## 🐛 حل المشاكل:

### "Cannot connect to server"
- ✅ تأكد من فتح المنفذ 1433 في Firewall
- ✅ تأكد من فتح المنفذ في OCI Security List
- ✅ تأكد من تفعيل TCP/IP في SQL Server

### "Login failed"
- ✅ تأكد من تفعيل SQL Server Authentication
- ✅ تأكد من صحة اسم المستخدم وكلمة المرور

---

## 📝 ملاحظات:

- **TrustServerCertificate=True**: للتطوير فقط
- للإنتاج: استخدم شهادة SSL صحيحة
- راقب استخدام الموارد والأداء
- أعد نسخ احتياطي يومي

---

**للمزيد من التفاصيل**: راجع `OCI_SETUP_GUIDE.md`

