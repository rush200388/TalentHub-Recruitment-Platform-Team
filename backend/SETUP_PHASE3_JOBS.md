# Phase 3 — Organizations, Departments, Skills, and Jobs

This phase adds real database-backed endpoints:

- GET/POST/PUT/DELETE /api/Organizations
- GET/POST/PUT/DELETE /api/Departments
- GET /api/Skills
- GET/POST/PUT/DELETE /api/Jobs
- Recruiter ownership checks
- Administrator controls
- Automatic skill creation
- Audit logs for create/update/delete
- Optional seeded recruiter, organization, department, and example job

## 1. Stop the API

Press Ctrl+C in the backend terminal.

## 2. Extract

Extract this ZIP into:

E:\SA\backend

Allow Windows to merge folders and replace files.

## 3. Configure a development recruiter

Run from:

E:\SA\backend\RecruitmentPlatform.Api

dotnet user-secrets set "SeedRecruiter:Email" "recruiter@recruitment.local"
dotnet user-secrets set "SeedRecruiter:Password" "Recruiter123"
dotnet user-secrets set "SeedRecruiter:Organization" "TalentHub Consulting"
dotnet user-secrets set "SeedRecruiter:Department" "Engineering"

Do not share the real password in screenshots or GitHub.

## 4. Build

Run from E:\SA\backend:

dotnet clean
dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj

## 5. Create the schema migration

This phase adds Job.WorkMode, so a migration is required:

dotnet ef migrations add AddJobWorkMode --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api --output-dir Data\Migrations

Apply it:

dotnet ef database update --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api

## 6. Start

dotnet run --project RecruitmentPlatform.Api

Open the Swagger URL.

You should see:

Organizations
Departments
Skills
Jobs

## 7. Test seeded accounts

Restarting the API seeds the recruiter if the four SeedRecruiter secrets exist.

Login:

POST /api/Auth/login

{
  "email": "recruiter@recruitment.local",
  "password": "Recruiter123"
}

Copy the token, click Authorize, and test:

GET /api/Jobs?mine=true
POST /api/Jobs

Candidate/public job browsing uses:

GET /api/Jobs

Only Published/Open jobs are returned publicly.
