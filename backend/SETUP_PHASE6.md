# Phase 6 — AI Resume Analysis, Administrator Users, Monitoring

This phase adds:

- PDF and DOCX resume text extraction
- Explainable rule-based skill extraction
- Email, phone, education, and experience-signal detection
- Apply extracted resume skills to the candidate profile
- Real administrator user creation and editing
- Secure role assignment
- Organization and department assignment
- User activation and suspension
- Failed-login and account-lock monitoring
- API, database, storage, and audit statistics

## 1. Stop the backend

Press Ctrl+C in the backend terminal.

## 2. Extract

Extract this ZIP into:

E:\SA\backend

Allow Windows to merge folders and replace files.

## 3. Install resume-processing packages

Run from E:\SA\backend, one command at a time:

dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package DocumentFormat.OpenXml --version 3.5.1

dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package UglyToad.PdfPig --version 1.7.0-custom-5

## 4. Build

dotnet clean

dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj

## 5. Create and apply the migration

Phase 6 adds analysis fields to the Resumes table:

dotnet ef migrations add AddResumeAnalysis --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api --output-dir Data\Migrations

dotnet ef database update --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api

## 6. Run

dotnet run --project RecruitmentPlatform.Api

New Swagger groups and endpoints:

Resume analysis:
- GET  /api/Candidates/me/resume/analysis
- POST /api/Candidates/me/resume/analyze
- POST /api/Candidates/me/resume/apply-skills

Administrator users:
- GET   /api/Users
- GET   /api/Users/{id}
- POST  /api/Users
- PUT   /api/Users/{id}
- PATCH /api/Users/{id}/status
- PATCH /api/Users/{id}/roles

Monitoring:
- GET /api/SystemMonitoring/health
- GET /api/SystemMonitoring/statistics

## AI description for the report

The prototype uses a replaceable Strategy interface:

IResumeAnalysisService
└── RuleBasedResumeAnalysisService

The rule-based strategy detects known skill aliases, contact patterns, education
signals, experience statements, and explicit years-of-experience phrases.

It does not claim to be a trained machine-learning model. Scanned image-only PDFs
need OCR and will return a clear warning in this prototype.
