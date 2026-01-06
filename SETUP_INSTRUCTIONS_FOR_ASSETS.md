# تعليمات إعداد Assets Module - للزميلة

## المشكلة
الكود الجديد يحتوي على:
- جداول Assets جديدة (Products, Vendors, AccessPoints, إلخ)
- صفحات Assets Dashboard وإدارة الأصول
- لكن **قاعدة البيانات عندك لا تحتوي على هذه الجداول**

---

## الحل الأسهل ⚡ (موصى به)

### ✨ الإعداد التلقائي - بدون أي شغل يدوي!

البرنامج الحين يسوي كل شي **تلقائياً** أول ما تشغليه!

#### الخطوة 1: Pull الكود الجديد
```bash
git pull origin Assets-page
```

#### الخطوة 2: شغلي البرنامج عادي
```bash
dotnet run
```

أو في VS Code:
```bash
dotnet watch run
```

**وخلاص! 🎉**

- البرنامج راح ينشئ كل الجداول المطلوبة تلقائياً
- ما راح يحذف أو يعدل أي جدول موجود (Tickets, Identity, إلخ)
- راح يشتغل مرة وحدة فقط (إذا الجداول موجودة ما يسوي شي)

---

## البديل اليدوي (إذا تفضليه)

### الخطوة 1: Pull الكود الجديد
```bash
git pull origin Assets-page
```

### الخطوة 2: تشغيل Migration Script اليدوي

افتحي **SQL Server Management Studio** أو أي أداة SQL وشغلي هذا السكريبت على قاعدة البيانات:

```sql
-- إنشاء جدول المنتجات (Products)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductType NVARCHAR(100) NOT NULL,
        ProductName NVARCHAR(150) NOT NULL,
        Manufacturer NVARCHAR(100) NOT NULL,
        PartNo NVARCHAR(50) NULL,
        Cost DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- إنشاء جدول الموردين (Vendors)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Vendors')
BEGIN
    CREATE TABLE Vendors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VendorName NVARCHAR(150) NOT NULL,
        Currency NVARCHAR(5) NOT NULL DEFAULT 'SR',
        DoorNumber NVARCHAR(10) NULL,
        Landmark NVARCHAR(100) NULL,
        PostalCode NVARCHAR(10) NULL,
        Country NVARCHAR(50) NULL,
        Fax NVARCHAR(20) NULL,
        FirstName NVARCHAR(50) NULL,
        Street NVARCHAR(100) NULL,
        City NVARCHAR(50) NULL,
        StateProvince NVARCHAR(50) NULL,
        PhoneNo NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END

-- إنشاء جدول حالات الأصول (AssetStates)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AssetStates')
BEGIN
    CREATE TABLE AssetStates (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Status INT NOT NULL DEFAULT 1,
        AssociatedTo NVARCHAR(50) NULL,
        Site NVARCHAR(50) NULL,
        StateComments NVARCHAR(500) NULL,
        UserId NVARCHAR(450) NULL,
        Department NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول تفاصيل الشبكة (NetworkDetails)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NetworkDetails')
BEGIN
    CREATE TABLE NetworkDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IPAddress NVARCHAR(15) NULL,
        MACAddress NVARCHAR(17) NULL,
        NIC NVARCHAR(50) NULL,
        Network NVARCHAR(100) NULL,
        DefaultGateway NVARCHAR(15) NULL,
        DHCPEnabled BIT NOT NULL DEFAULT 0,
        DHCPServer NVARCHAR(15) NULL,
        DNSHostname NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول معلومات الكمبيوتر (ComputerInfos)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComputerInfos')
BEGIN
    CREATE TABLE ComputerInfos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ServiceTag NVARCHAR(50) NULL,
        Manufacturer NVARCHAR(100) NULL,
        BiosDate DATETIME2 NULL,
        Domain NVARCHAR(100) NULL,
        SMBiosVersion NVARCHAR(50) NULL,
        BiosVersion NVARCHAR(50) NULL,
        BiosManufacturer NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول معلومات نظام التشغيل (OperatingSystemInfos)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OperatingSystemInfos')
BEGIN
    CREATE TABLE OperatingSystemInfos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NULL,
        Version NVARCHAR(50) NULL,
        BuildNumber NVARCHAR(50) NULL,
        ServicePack NVARCHAR(50) NULL,
        ProductId NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول تفاصيل الذاكرة (MemoryDetails)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MemoryDetails')
BEGIN
    CREATE TABLE MemoryDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RAM INT NULL,
        VirtualMemory INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول المعالجات (Processors)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Processors')
BEGIN
    CREATE TABLE Processors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProcessorInfo NVARCHAR(150) NULL,
        Manufacturer NVARCHAR(100) NULL,
        ClockSpeedMHz INT NULL,
        NumberOfCores INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول الأقراص الصلبة (HardDisks)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HardDisks')
BEGIN
    CREATE TABLE HardDisks (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Model NVARCHAR(100) NULL,
        SerialNumber NVARCHAR(100) NULL,
        Manufacturer NVARCHAR(100) NULL,
        CapacityGB INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول لوحات المفاتيح (Keyboards)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Keyboards')
BEGIN
    CREATE TABLE Keyboards (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        KeyboardType NVARCHAR(100) NULL,
        Manufacturer NVARCHAR(100) NULL,
        SerialNumber NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول الفأرة (Mice)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Mice')
BEGIN
    CREATE TABLE Mice (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MouseType NVARCHAR(100) NULL,
        SerialNumber NVARCHAR(100) NULL,
        MouseButtons INT NULL,
        Manufacturer NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول الشاشات (Monitors)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Monitors')
BEGIN
    CREATE TABLE Monitors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        MonitorType NVARCHAR(100) NULL,
        SerialNumber NVARCHAR(100) NULL,
        Manufacturer NVARCHAR(100) NULL,
        MaxResolution NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول تفاصيل الأجهزة المحمولة (MobileDetails)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MobileDetails')
BEGIN
    CREATE TABLE MobileDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IMEI NVARCHAR(20) NULL,
        Model NVARCHAR(100) NULL,
        ModelNo NVARCHAR(100) NULL,
        TotalCapacityGB INT NULL,
        AvailableCapacityGB INT NULL,
        ModemFirmwareVersion NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
END

-- إنشاء جدول الأصول الرئيسي (Assets) - TPH Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets')
BEGIN
    CREATE TABLE Assets (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AssetType NVARCHAR(100) NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        ProductId INT NOT NULL,
        SerialNumber NVARCHAR(100) NULL,
        AssetTag NVARCHAR(50) NULL,
        VendorId INT NULL,
        PurchaseCost DECIMAL(18, 2) NOT NULL DEFAULT 0,
        ExpiryDate DATETIME2 NULL,
        Location NVARCHAR(150) NULL,
        AcquisitionDate DATETIME2 NULL,
        WarrantyExpiryDate DATETIME2 NULL,
        AssetStateId INT NULL,
        NetworkDetailsId INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CreatedById NVARCHAR(450) NULL,
        
        -- Foreign Keys
        CONSTRAINT FK_Assets_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
        CONSTRAINT FK_Assets_Vendors FOREIGN KEY (VendorId) REFERENCES Vendors(Id),
        CONSTRAINT FK_Assets_AssetStates FOREIGN KEY (AssetStateId) REFERENCES AssetStates(Id),
        CONSTRAINT FK_Assets_NetworkDetails FOREIGN KEY (NetworkDetailsId) REFERENCES NetworkDetails(Id)
    );
END

PRINT 'Assets tables created successfully!';
```

### الخطوة 3: تأكدي من نجاح العملية
شغلي هذا الاستعلام للتأكد:
```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('Products', 'Vendors', 'Assets', 'AssetStates', 'NetworkDetails')
ORDER BY TABLE_NAME;
```

لازم تشوفي 5 جداول.

### الخطوة 4: شغلي البرنامج
```bash
dotnet run
```

أو في VS Code:
```bash
dotnet watch run
```

---

## التأكد من نجاح الإعداد ✅

1. افتحي المتصفح: `https://localhost:5001/Assets/Dashboard`
2. لازم تشوفي صفحة Dashboard للأصول (فاضية حالياً)
3. اضغطي على أي نوع من الأصول (مثلاً Access Points)
4. لازم تفتح صفحة فاضية بدون أخطاء

---

## إذا واجهتي مشاكل ❗

### مشكلة: Build Errors
```bash
dotnet clean
dotnet build
```

### مشكلة: Foreign Key Constraints
إذا طلع error في Foreign Keys، شغلي السكريبت بدون الـ CONSTRAINT statements (احذفي الأسطر اللي فيها FK_)

### مشكلة: الجداول موجودة مسبقاً
لا مشكلة! السكريبت فيه `IF NOT EXISTS` - ما راح يعمل شي

---

## ملاحظات مهمة 📝

1. **لا تحذفي** جداول Tickets أو Identity - هي موجودة وتشتغل
2. **السكريبت آمن** - ما يحذف أو يعدل أي بيانات موجودة
3. **الجداول الجديدة فقط** للـ Assets Module
4. **بعد ما تشتغل عندك** - أي تعديلات مستقبلية راح تجي عن طريق Migrations عادي

---

## للتواصل
إذا فيه أي مشكلة، تواصلي مع عبدالرزاق
