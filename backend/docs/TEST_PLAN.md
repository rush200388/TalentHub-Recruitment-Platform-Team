# Phase 7 Test Plan

## Automated tests

Run:

```cmd
dotnet test RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj --logger "console;verbosity=normal"
```

Evidence to capture:

- Total tests
- Passed tests
- Failed tests
- Test execution time

The supplied tests cover:

- Candidate-job matching business rules
- Resume skill/contact extraction
- Automated feedback fallback rules
- DTO validation
- Registration API
- Duplicate account handling
- JWT authentication
- Role-based authorization
- 401 and 403 responses

## Manual UAT workflow

1. Candidate registers and signs in.
2. Candidate completes the profile.
3. Candidate uploads and analyzes a resume.
4. Candidate applies for a published job.
5. Recruiter reviews and shortlists the candidate.
6. Recruiter schedules an interview.
7. Hiring Manager submits feedback and an evaluation.
8. Recruiter or Hiring Manager generates AI candidate feedback.
9. Hiring Manager records the hiring decision.
10. Candidate receives application and interview notifications.
11. Administrator reviews users, analytics, audit logs, and monitoring.

## Negative tests

- Invalid registration data returns 400.
- Duplicate registration returns 409.
- No JWT token returns 401.
- Candidate accessing administrator API returns 403.
- Invalid 10-digit phone number returns 400.
- Duplicate application returns 409.
- Invalid or conflicting interview schedule is rejected.
- Unsupported resume file type is rejected.
- Suspended user cannot sign in.
