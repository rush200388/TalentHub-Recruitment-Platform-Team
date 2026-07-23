# Member 4 — Administrator, Analytics, and Monitoring

    **Git branch:** `feature/admin-monitoring`  
    **Assigned source files:** 30

    ## Responsibilities

    - Administrator dashboard and navigation
- User and role management
- Analytics, notifications, audit logs, and system monitoring
- Application shell and shared administrator-facing UI

    ## How to use this package

    1. Clone the shared repository.
    2. Create the branch `feature/admin-monitoring`.
    3. Extract/copy the `backend` and `frontend` folders from this package into the cloned repository.
    4. Review and test the assigned module.
    5. Commit only genuine work performed by this member.
    6. Push the branch and create a Pull Request into `main`.

    This package is a module assignment, not a standalone complete application. The complete application is produced after all six branches are merged.

    ## Assigned files

    - `backend/RecruitmentPlatform.Api/Controllers/AnalyticsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/AuditLogsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/NotificationsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/SystemMonitoringController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/UsersController.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Analytics/AnalyticsDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Audit/AuditLogDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Monitoring/MonitoringDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Notifications/NotificationDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Users/UserManagementDtos.cs`
- `backend/RecruitmentPlatform.Domain/Entities/AuditLog.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Notification.cs`
- `backend/SETUP_PHASE6.md`
- `frontend/SETUP_FRONTEND_PHASE6.md`
- `frontend/src/App.jsx`
- `frontend/src/components/layout/DashboardLayout.jsx`
- `frontend/src/components/layout/Header.jsx`
- `frontend/src/components/layout/Sidebar.jsx`
- `frontend/src/index.css`
- `frontend/src/main.jsx`
- `frontend/src/pages/NotFoundPage.jsx`
- `frontend/src/pages/ProfilePage.jsx`
- `frontend/src/pages/UnauthorizedPage.jsx`
- `frontend/src/pages/admin/AdminDashboard.jsx`
- `frontend/src/pages/admin/AnalyticsPage.jsx`
- `frontend/src/pages/admin/AuditLogsPage.jsx`
- `frontend/src/pages/admin/DepartmentsPage.jsx`
- `frontend/src/pages/admin/MonitoringPage.jsx`
- `frontend/src/pages/admin/OrganizationsPage.jsx`
- `frontend/src/pages/admin/UsersPage.jsx`
