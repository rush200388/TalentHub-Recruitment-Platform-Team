# Frontend Phase 4 — Candidate Profiles, Applications, AI Matching

## 1. Stop the frontend

Press Ctrl+C in the frontend terminal.

## 2. Extract

Extract this ZIP into:

E:\SA\front end\project

Allow Windows to merge folders and replace files.

## 3. Start both applications

Backend terminal:

cd /d E:\SA\backend
dotnet run --project RecruitmentPlatform.Api

Frontend terminal:

cd /d "E:\SA\front end\project"
npm run dev

## 4. Candidate test

Log in or register as a Candidate.

Candidate → Profile:
- Add title, location, experience, summary, and skills
- Upload a PDF or DOCX resume
- Save the profile

Candidate → Browse Jobs:
- Open a published job
- Submit an application

Candidate → My Applications:
- Confirm the application, stage, status, and AI score

Candidate → Recommendations:
- Confirm jobs are ranked by match score
- Check matched and missing skills

## 5. Recruiter test

Log in with the seeded recruiter.

Recruiter → Applicant Review:
- View real applicants for the recruiter's jobs
- Open an application
- Review matched and missing skills
- Shortlist or reject the applicant

## Important

The matching feature is explainable and rule-based:

- 80% required skill match
- 20% experience match

Describe it as a rule-based AI strategy in the report, not as a trained ML model.
