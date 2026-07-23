# Frontend Phase 6 — Resume AI, Real Users, Monitoring

## 1. Stop the frontend

Press Ctrl+C.

## 2. Extract

Extract this ZIP into:

E:\SA\front end\project

Allow Windows to merge folders and replace files.

## 3. Build and run

cd /d "E:\SA\front end\project"

npm run build

npm run dev

## 4. Candidate test

- Upload a PDF or DOCX from Candidate → My Profile
- Open Candidate → AI Resume Analysis
- Click Analyze Resume
- Review extracted skills, email, phone, education, experience, and warnings
- Select skills and click Apply Selected
- Return to My Profile and confirm the skills were added

## 5. Administrator user test

- Open Administrator → Users & Roles
- Create a Candidate, Recruiter, Hiring Manager, or Administrator
- Recruiter and Hiring Manager require an organization
- Edit role and assignment
- Suspend and activate the user
- Confirm last-login, failed-login, and lock information

## 6. Monitoring test

Open Administrator → System Monitoring.

Confirm:

- API and PostgreSQL status
- Active/inactive/locked users
- Failed login count
- Stored and analyzed resumes
- Jobs, applications, interviews, and audit records
- Recent system activity
