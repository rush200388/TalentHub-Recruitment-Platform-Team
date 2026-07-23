# Frontend Phase 3 — Real Organizations, Departments, and Jobs

## 1. Stop the frontend

Press Ctrl+C in the frontend terminal.

## 2. Extract

Extract this ZIP into:

E:\SA\front end\project

Allow Windows to merge folders and replace existing files.

## 3. Start both applications

Backend terminal:

cd /d E:\SA\backend
dotnet run --project RecruitmentPlatform.Api

Frontend terminal:

cd /d "E:\SA\front end\project"
npm run dev

## 4. Test recruiter job management

Log in with the recruiter account configured in backend user secrets.

Open:

Recruiter → Manage Jobs

Create an Open/Published job. It should appear in PostgreSQL and immediately
be visible to candidates.

## 5. Test candidate browsing

Register or log in as a Candidate.

Open:

Candidate → Browse Jobs

Only Open/Published jobs are loaded from GET /api/Jobs.

## 6. Test administrator management

Log in with the seeded administrator account.

Open:

Administrator → Organizations
Administrator → Departments

Create and edit records. Deletion is blocked when dependent jobs or departments
exist.

## Important

Application submission is still temporary/mock behavior. Phase 4 will connect:

- Candidate profiles and skills
- Job applications
- Skill-based AI match scores
- My Applications
- Recruiter candidate review
