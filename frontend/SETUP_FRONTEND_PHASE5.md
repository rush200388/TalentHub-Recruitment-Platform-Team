# Frontend Phase 5 — Interview and Hiring Workflow

## 1. Extract

Stop the frontend and extract this ZIP into:

E:\SA\front end\project

Allow Windows to merge folders and replace files.

## 2. Start both applications

Backend:

cd /d E:\SA\backend
dotnet run --project RecruitmentPlatform.Api

Frontend:

cd /d "E:\SA\front end\project"
npm run build
npm run dev

## 3. Test workflow

Recruiter:
- Open Applicant Review and shortlist a candidate
- Open Interviews
- Select the candidate and seeded hiring manager
- Choose future date/time and schedule

Hiring Manager:
- Open Interviews and submit 1–5 feedback scores
- Open Shortlisted and save the 0–100 formal evaluation
- Open Hiring Decisions and choose Hire, Reject, or On Hold

Candidate:
- Open the notification bell in the header
- Check interview and decision notifications
- Check My Applications for the updated stage/status

Administrator:
- Open Analytics
- Open Audit Logs

## 4. Email

The frontend does not need SMTP credentials. Email is configured only in the
backend user-secrets store. Internal notifications work even when SMTP is off.
