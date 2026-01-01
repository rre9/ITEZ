# 🔄 شرح Workflow النظام - من يوافق ومتى؟

## 📋 **Access Request Workflow (طلب الوصول)**

### **المرحلة 1: Employee ينشئ الطلب**
```
👤 Employee (أي موظف)
├─ يملأ نموذج Access Request
├─ يختار Manager للموافقة (SelectedManagerId)
├─ Ticket.Status = New
├─ Ticket.AssignedToId = SelectedManagerId (المدير المختار)
└─ AccessRequest.ManagerApprovalStatus = Pending
```

**أين يظهر:**
- Employee يرى الطلب في `/Tickets/MyTickets`
- Manager المختار يرى الطلب في `/Tickets/MyTasks`

---

### **المرحلة 2: Manager Approval (موافقة المدير)**

**من يوافق:** 👔 **Manager** (المدير المختار في الطلب)

**الشروط:**
- `User.IsInRole("Manager")` = true
- `AccessRequest.SelectedManagerId == Current User Id`
- `AccessRequest.ManagerApprovalStatus == Pending`

**السيناريوهات:**

#### ✅ **إذا وافق Manager:**
```
1. Manager يضغط "Approve" في ApproveAccessRequest
2. AccessRequest.ManagerApprovalStatus = Approved
3. AccessRequest.ManagerApprovalDate = Now
4. AccessRequest.ManagerApprovalName = Manager.FullName
5. Ticket.Status = InProgress

6. 🔍 Check: هل Mohammed (Security) هو Creator؟
   ├─ ✅ نعم (Mohammed أنشأ الطلب):
   │   ├─ AccessRequest.SecurityApprovalStatus = Approved (Auto)
   │   ├─ AccessRequest.SecurityApprovalDate = Now
   │   ├─ AccessRequest.SecurityApprovalName = "Security (Auto-approved)"
   │   ├─ Ticket.AssignedToId = IT User (Yazan)
   │   └─ 🎯 ينتقل مباشرة لـ IT (يتخطى Security)
   │
   └─ ❌ لا (موظف عادي أنشأ الطلب):
       ├─ Ticket.AssignedToId = Security User (Mohammed)
       └─ 🎯 ينتقل لـ Security للموافقة
```

#### ❌ **إذا رفض Manager:**
```
1. Manager يضغط "Reject" في RejectAccessRequest
2. AccessRequest.ManagerApprovalStatus = Rejected
3. AccessRequest.ManagerApprovalDate = Now
4. Ticket.Status = Rejected
5. 🛑 Workflow يتوقف تماماً
6. لا ينتقل لـ Security أو IT
7. Employee يرى الطلب في MyTickets بحالة Rejected
```

**أين يظهر بعد الموافقة:**
- Security (Mohammed) يرى الطلب في `/Tickets/MyTasks` (إذا لم يكن Mohammed هو Creator)
- أو IT (Yazan) يرى الطلب مباشرة (إذا كان Mohammed هو Creator)

---

### **المرحلة 3: Security Approval (موافقة الأمن)**

**من يوافق:** 🛡️ **Security** (Mohammed - mohammed.cyber@yub.com.sa)

**الشروط:**
- `User.IsInRole("Security")` = true
- `AccessRequest.ManagerApprovalStatus == Approved` (Manager وافق)
- `AccessRequest.SecurityApprovalStatus == Pending`

**ملاحظة مهمة:**
- إذا كان Mohammed هو Creator → هذه المرحلة تُتخطى تلقائياً
- إذا كان Mohammed ليس Creator → يجب أن يوافق Security

**السيناريوهات:**

#### ✅ **إذا وافق Security:**
```
1. Security (Mohammed) يضغط "Approve" في ApproveSecurityAccess
2. AccessRequest.SecurityApprovalStatus = Approved
3. AccessRequest.SecurityApprovalDate = Now
4. AccessRequest.SecurityApprovalName = Security.FullName
5. Ticket.AssignedToId = IT User (Yazan)
6. Ticket.Status = InProgress
7. 🎯 ينتقل لـ IT للمراجعة النهائية
```

#### ❌ **إذا رفض Security:**
```
1. Security يضغط "Reject" في RejectSecurityAccess
2. AccessRequest.SecurityApprovalStatus = Rejected
3. AccessRequest.SecurityApprovalDate = Now
4. Ticket.Status = Rejected
5. 🛑 Workflow يتوقف تماماً
6. لا ينتقل لـ IT
7. Employee يرى الطلب في MyTickets بحالة Rejected
```

**أين يظهر بعد الموافقة:**
- IT (Yazan) يرى الطلب في `/Tickets/MyTasks` أو `/Tickets` (Index)

---

### **المرحلة 4: IT Review (المراجعة النهائية)**

**من يوافق:** 💻 **IT** (Yazan - yazan@yub.com.sa)

**الشروط:**
- `User.IsInRole("IT")` = true
- `Ticket.Status == InProgress`
- `Ticket.AssignedToId == Current IT User Id` ⚠️ **مهم جداً**
- `AccessRequest.ManagerApprovalStatus == Approved`
- `AccessRequest.SecurityApprovalStatus == Approved`

**السيناريوهات:**

#### ✅ **إذا وافق IT (Approve & Complete):**
```
1. IT (Yazan) يضغط "Approve & Complete" في ReviewIT
2. AccessRequest.ITApprovalStatus = Approved
3. AccessRequest.ITApprovalDate = Now
4. AccessRequest.ITApprovalName = IT.FullName
5. Ticket.Status = Resolved
6. 🎉 Workflow مكتمل
7. التذكرة تُقفل نهائياً - لا يمكن تعديلها
8. Employee يرى الطلب في MyTickets بحالة Resolved
```

#### ❌ **إذا رفض IT (Reject & Close):**
```
1. IT يضغط "Reject & Close" في ReviewIT
2. AccessRequest.ITApprovalStatus = Rejected
3. AccessRequest.ITApprovalDate = Now
4. Ticket.Status = Rejected
5. 🛑 Workflow مكتمل
6. التذكرة تُقفل نهائياً - لا يمكن تعديلها
7. Employee يرى الطلب في MyTickets بحالة Rejected
```

**أين يظهر:**
- IT (Yazan) يرى الطلب في:
  - `/Tickets/MyTasks` - مع زر "Review"
  - `/Tickets` (Index - Ticket Dashboard) - مع زر "Review" بجانب "View Details"

---

## 📋 **Service Request Workflow (طلب الخدمة)**

### **المرحلة 1: Employee ينشئ الطلب**
```
👤 Employee
├─ يملأ نموذج Service Request
├─ يختار Manager للموافقة
├─ Ticket.Status = New
├─ Ticket.AssignedToId = SelectedManagerId
└─ ServiceRequest.ManagerApprovalStatus = Pending
```

---

### **المرحلة 2: Manager Approval**

**من يوافق:** 👔 **Manager** (المدير المختار)

**السيناريوهات:**

#### ✅ **إذا وافق Manager:**
```
1. ServiceRequest.ManagerApprovalStatus = Approved
2. Ticket.AssignedToId = Security (Mohammed)
3. Ticket.Status = InProgress
4. 🎯 ينتقل لـ Security
```

#### ❌ **إذا رفض Manager:**
```
1. ServiceRequest.ManagerApprovalStatus = Rejected
2. Ticket.Status = Rejected
3. 🛑 Workflow يتوقف
```

---

### **المرحلة 3: Security Approval**

**من يوافق:** 🛡️ **Security** (Mohammed)

**السيناريوهات:**

#### ✅ **إذا وافق Security:**
```
1. ServiceRequest.SecurityApprovalStatus = Approved
2. Ticket.AssignedToId = IT User (Yazan)
3. Ticket.Status = InProgress
4. 🎯 ينتقل لـ IT للتنفيذ
```

#### ❌ **إذا رفض Security:**
```
1. ServiceRequest.SecurityApprovalStatus = Rejected
2. Ticket.Status = Rejected
3. 🛑 Workflow يتوقف
```

---

### **المرحلة 4: IT Execution**

**من ينفذ:** 💻 **IT** (Yazan)

**السيناريوهات:**

#### ✅ **إذا Execute (Complete):**
```
1. ServiceRequest.ITApprovalStatus = Approved
2. Ticket.Status = Resolved
3. 🎉 Workflow مكتمل
```

#### ❌ **إذا Close (Reject):**
```
1. ServiceRequest.ITApprovalStatus = Rejected
2. Ticket.Status = Rejected
3. 🛑 Workflow مكتمل
```

---

## 🎯 **ملخص الأدوار والصلاحيات**

| الدور | من يوافق | متى يظهر في MyTasks | متى يظهر Review Button |
|------|---------|-------------------|---------------------|
| **Employee** | ❌ لا يوافق | ✅ تذاكره فقط | ❌ لا |
| **Manager** | ✅ Access/Service Requests (إذا كان SelectedManager) | ✅ التذاكر المخصصة له | ✅ عندما ManagerApprovalStatus = Pending |
| **Security** | ✅ Access/Service Requests (بعد Manager Approval) | ✅ التذاكر المخصصة له | ✅ عندما SecurityApprovalStatus = Pending |
| **IT** | ✅ Access Requests (Review) + Service Requests (Execute) | ✅ التذاكر المخصصة له | ✅ عندما Manager + Security Approved + AssignedToId = Current User |

---

## ⚠️ **نقاط مهمة جداً**

### 1. **Identity Consistency (اتساق الهوية)**
```
⚠️ الشرط الأهم للـ IT Review:
Ticket.AssignedToId == Current User Id

إذا كان:
- Ticket.AssignedToId = User A
- Login Session = User B
→ Review لن يظهر ❌

الحل: يجب أن يكون نفس UserId
```

### 2. **Mohammed Creator Exception**
```
إذا كان Mohammed (Security) هو Creator:
→ Security Approval يُتخطى تلقائياً
→ ينتقل مباشرة لـ IT
```

### 3. **Workflow Stops on Rejection**
```
أي رفض في أي مرحلة:
→ Ticket.Status = Rejected
→ Workflow يتوقف تماماً
→ لا ينتقل للمرحلة التالية
```

### 4. **IT Review is Final**
```
بعد IT Review (Approve/Reject):
→ التذكرة تُقفل نهائياً
→ لا يمكن تعديلها
→ لا يمكن إعادة فتحها
```

---

## 📍 **أين يظهر كل شيء؟**

### **Employee:**
- `/Tickets/MyTickets` - تذاكره فقط

### **Manager:**
- `/Tickets/MyTasks` - التذاكر المخصصة له للموافقة
- `/Tickets/Index` - جميع التذاكر (إذا كان Support/Admin)

### **Security:**
- `/Tickets/MyTasks` - التذاكر المخصصة له للموافقة
- `/Tickets/Index` - جميع التذاكر

### **IT:**
- `/Tickets/MyTasks` - التذاكر المخصصة له مع زر "Review"
- `/Tickets/Index` - جميع التذاكر مع زر "Review" بجانب "View Details"
- `/Tickets/ReviewIT/{id}` - صفحة IT Review المخصصة

---

## 🔄 **مثال عملي كامل:**

```
1. Employee (أحمد) ينشئ Access Request
   → يختار Manager: Mashael IT R
   → Ticket.AssignedToId = Mashael's UserId
   → يظهر في MyTasks لـ Mashael

2. Mashael (Manager) يوافق
   → AccessRequest.ManagerApprovalStatus = Approved
   → Ticket.AssignedToId = Mohammed's UserId (Security)
   → يظهر في MyTasks لـ Mohammed

3. Mohammed (Security) يوافق
   → AccessRequest.SecurityApprovalStatus = Approved
   → Ticket.AssignedToId = Yazan's UserId (IT)
   → يظهر في MyTasks و Index لـ Yazan مع زر "Review"

4. Yazan (IT) يضغط "Review"
   → يفتح صفحة ReviewIT
   → يختار Approve & Complete
   → Ticket.Status = Resolved
   → 🎉 Workflow مكتمل
```

---

**تم الشرح بواسطة: Senior Software Engineer + System Analyst**

