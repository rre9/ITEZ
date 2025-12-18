-- =============================================
-- Script لإنشاء قاعدة البيانات والمستخدم على OCI SQL Server
-- =============================================
-- استخدم هذا الملف في SQL Server Management Studio (SSMS)
-- بعد تفعيل SQL Server Authentication
-- =============================================

-- إنشاء قاعدة البيانات
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ITHelpDesk')
BEGIN
    CREATE DATABASE ITHelpDesk;
    PRINT 'تم إنشاء قاعدة البيانات ITHelpDesk بنجاح';
END
ELSE
BEGIN
    PRINT 'قاعدة البيانات ITHelpDesk موجودة بالفعل';
END
GO

-- استخدام قاعدة البيانات
USE ITHelpDesk;
GO

-- إنشاء Login (استبدل كلمة المرور بكلمة مرور قوية!)
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'ithelpdesk_user')
BEGIN
    CREATE LOGIN ithelpdesk_user WITH PASSWORD = 'ChangeThisPassword123!';
    PRINT 'تم إنشاء Login ithelpdesk_user بنجاح';
END
ELSE
BEGIN
    PRINT 'Login ithelpdesk_user موجود بالفعل';
END
GO

-- إنشاء User في قاعدة البيانات
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'ithelpdesk_user')
BEGIN
    CREATE USER ithelpdesk_user FOR LOGIN ithelpdesk_user;
    PRINT 'تم إنشاء User ithelpdesk_user بنجاح';
END
ELSE
BEGIN
    PRINT 'User ithelpdesk_user موجود بالفعل';
END
GO

-- إعطاء الصلاحيات الكاملة (db_owner)
-- يمكنك تغيير هذا إلى db_datareader, db_datawriter حسب الحاجة
ALTER ROLE db_owner ADD MEMBER ithelpdesk_user;
GO

PRINT '=============================================';
PRINT 'تم إعداد قاعدة البيانات بنجاح!';
PRINT '=============================================';
PRINT 'اسم المستخدم: ithelpdesk_user';
PRINT 'كلمة المرور: ChangeThisPassword123!';
PRINT '';
PRINT '⚠️  مهم: غير كلمة المرور قبل النشر!';
PRINT '=============================================';

