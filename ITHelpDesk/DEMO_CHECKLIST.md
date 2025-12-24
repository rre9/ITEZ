# IT Help Desk - Demo Checklist

## ✅ Core Features (All Working)

### 1. User Management
- [x] User registration (no email confirmation required)
- [x] Login/Logout
- [x] Password reset (with email link in dev mode)
- [x] Admin Panel for user management
- [x] Role-based access (Admin, Support, Employee)

### 2. Ticket Management
- [x] Create tickets (all users)
- [x] View tickets (with authorization)
- [x] Update ticket status
- [x] Assign/transfer tickets
- [x] Upload attachments
- [x] Activity log tracking

### 3. Workflow
- [x] New tickets auto-assigned to yazan@yub.com.sa
- [x] Admin/Support can create and assign tickets
- [x] Assigned users can update status
- [x] Email notifications on assignment/status change
- [x] Tasks page for assigned tickets

### 4. Security
- [x] Resource-based authorization
- [x] Email domain validation (@yub.com.sa)
- [x] Role-based policies
- [x] Ticket access control

## ⚠️ Before Demo - Important Checks

### 1. Email Configuration
**CRITICAL:** Update `appsettings.Development.json` with real SMTP credentials:
```json
"EmailSettings": {
  "Host": "smtp.office365.com",
  "Port": 587,
  "UserName": "REAL_EMAIL@yub.com.sa",
  "Password": "REAL_PASSWORD",
  "From": "it-helpdesk@yub.com.sa"
}
```

### 2. Database
- [ ] Ensure database is created and migrated
- [ ] Verify admin account exists (yazan@yub.com.sa / Admin#12345!)

### 3. Departments
Current departments include Arabic names. Consider updating to English only:
- المالية → Finance
- السايبر → Cybersecurity
- العمليات التقنية → Technical Operations
- الموارد البشرية → Human Resources

### 4. Testing Scenarios for Demo

#### Scenario 1: Employee Creates Ticket
1. Login as Employee (e.g., r.aslami@yub.com.sa)
2. Go to "Submit Ticket"
3. Fill form and submit
4. Verify ticket appears in "My Tickets"
5. Verify ticket auto-assigned to yazan@yub.com.sa

#### Scenario 2: Admin Assigns Ticket
1. Login as Admin (yazan@yub.com.sa)
2. Go to "Tasks" - see new ticket
3. Click "Update Status"
4. Assign to another user (e.g., r.aslami@yub.com.sa)
5. Add internal notes
6. Save changes
7. Verify email sent to assigned user

#### Scenario 3: Assigned User Updates Status
1. Login as assigned user (r.aslami@yub.com.sa)
2. Go to "Tasks" - see assigned ticket
3. Click ticket to view details
4. Click "Update Status"
5. Change status to "InProgress" or "Resolved"
6. Add notes
7. Save changes

#### Scenario 4: Admin Creates and Assigns Ticket
1. Login as Admin
2. Go to "Submit Ticket"
3. Fill form
4. Select assignee from "Assign To" dropdown
5. Submit
6. Verify ticket appears in assignee's "Tasks"

## 📋 Demo Flow Recommendation

1. **Start with Admin Panel**
   - Show user management
   - Show roles

2. **Create Ticket as Employee**
   - Show auto-assignment
   - Show email notification

3. **Admin Workflow**
   - Show "Tasks" page
   - Show "All Tickets" page
   - Assign ticket to someone
   - Show email notification

4. **Assigned User Workflow**
   - Show "Tasks" page
   - Update status
   - Add notes

5. **Security Features**
   - Try accessing ticket without permission (403)
   - Show authorization working

## 🔧 Quick Fixes if Needed

### If emails not sending:
- Check SMTP credentials in appsettings.Development.json
- Verify network/firewall allows SMTP
- Check logs for errors

### If admin password wrong:
- Password is: `Admin#12345!`
- Or check `appsettings.Development.json` → `Seed:AdminDefaultPassword`

### If database issues:
- Run: `dotnet ef database update`
- Or delete database and restart app (will auto-create)

## 📝 Notes
- All UI text is in English
- Email notifications are in English
- System is ready for production after SMTP configuration

