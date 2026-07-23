# Phase 5 — Interviews, Feedback, Decisions, Notifications, Email, Analytics

This phase adds real PostgreSQL-backed:

- Hiring-manager directory
- Interview scheduling and conflict detection
- Interview rescheduling/cancellation
- Interview feedback and recommendations
- Candidate evaluation scoring
- Hiring decisions
- Internal notifications
- Optional SMTP email
- ICS calendar invitations
- Recruitment analytics
- Real audit-log endpoint

## 1. Extract

Stop the API and extract this ZIP into:

E:\SA\backend

Allow Windows to merge folders and replace files.

## 2. Configure a hiring-manager account

Run from:

E:\SA\backend\RecruitmentPlatform.Api

Run each command separately:

dotnet user-secrets set "SeedManager:Email" "manager@recruitment.local"

dotnet user-secrets set "SeedManager:Password" "Manager123"

dotnet user-secrets set "SeedManager:Organization" "TalentHub Consulting"

dotnet user-secrets set "SeedManager:Department" "Engineering"

The manager organization and department should match the recruiter and job.

## 3. Optional SMTP email

Internal notifications and calendar data work without email.

To enable SMTP, set:

dotnet user-secrets set "Email:Enabled" "true"
dotnet user-secrets set "Email:Host" "YOUR_SMTP_HOST"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:EnableSsl" "true"
dotnet user-secrets set "Email:Username" "YOUR_SMTP_USERNAME"
dotnet user-secrets set "Email:Password" "YOUR_SMTP_PASSWORD"
dotnet user-secrets set "Email:FromEmail" "YOUR_FROM_EMAIL"
dotnet user-secrets set "Email:FromName" "TalentHub Recruitment"

Do not put SMTP passwords in appsettings.json or GitHub.

To keep email disabled:

dotnet user-secrets set "Email:Enabled" "false"

## 4. Build

From E:\SA\backend:

dotnet clean

dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj

## 5. Create and apply migration

This phase adds Interview.InterviewerUserId and useful unique/index constraints.

dotnet ef migrations add AddInterviewWorkflow --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api --output-dir Data\Migrations

dotnet ef database update --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api

## 6. Run

dotnet run --project RecruitmentPlatform.Api

Open the Swagger URL printed in the terminal.

New Swagger groups:

- HiringManagers
- Interviews
- Evaluations
- HiringDecisions
- Notifications
- Analytics
- AuditLogs

## 7. Test order

1. Candidate applies for a published job.
2. Recruiter shortlists the candidate.
3. Recruiter schedules an interview and assigns the seeded manager.
4. Manager submits interview feedback.
5. Manager saves a formal evaluation.
6. Manager records Hire, Reject, or On Hold.
7. Candidate checks the notification bell.
8. Administrator opens Analytics and Audit Logs.

## Scoring

Formal evaluation:

Overall =
Skills × 30%
+ Experience × 20%
+ Interview × 50%

The earlier AI job match remains:

AI match =
Required skills × 80%
+ Experience compatibility × 20%
