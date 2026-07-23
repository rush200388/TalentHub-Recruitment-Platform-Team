# Phase 4 — Candidate Profiles, Applications, Applicant Review, AI Matching

This phase adds:

- Real candidate profile read/update
- Candidate skills stored in PostgreSQL
- Real PDF/DOCX resume upload and download
- Real job application submission
- Duplicate application prevention
- Candidate application tracking
- Recruiter applicant review
- Shortlist and reject actions
- Transparent rule-based AI match score
- Job recommendations ranked by match score
- Notifications and audit logs

## 1. Stop the API

Press Ctrl+C in the backend terminal.

## 2. Extract

Extract this ZIP into:

E:\SA\backend

Allow Windows to merge folders and replace existing files.

## 3. Build

From E:\SA\backend, run one command at a time:

dotnet clean

dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj

## 4. Migration

No new migration is required in this phase. It uses the existing:

- CandidateProfiles
- CandidateSkills
- Skills
- Resumes
- JobApplications
- Notifications
- AuditLogs

tables.

## 5. Start

dotnet run --project RecruitmentPlatform.Api

Open:

http://localhost:5117/swagger

The exact port may differ.

## 6. Swagger endpoints

Candidates:
- GET /api/Candidates/me
- PUT /api/Candidates/me
- POST /api/Candidates/me/resume
- GET /api/Candidates/me/resume/{resumeId}/download
- GET /api/Candidates/me/recommendations
- GET /api/Candidates
- GET /api/Candidates/{id}

Applications:
- GET /api/Applications
- GET /api/Applications/{id}
- POST /api/Applications
- PATCH /api/Applications/{id}/status

## 7. AI scoring approach

The prototype uses an explainable strategy:

- 80% required-skill match
- 20% minimum-experience match
- Result is limited to 0–100

This is suitable for a coursework prototype and must be described as a
rule-based AI matching strategy, not a trained machine-learning model.
