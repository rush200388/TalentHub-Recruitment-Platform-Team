# TalentHub Team Contribution Workflow

This starter repository intentionally contains only shared team files. Do not upload the completed backend or frontend to `main` before the six members contribute.

## Branch and module ownership

| Member | Module | Branch | Assigned source files |
|---:|---|---|---:|
| 1 | Candidate portal, applications, resume analysis, and storage | `feature/candidate-portal` | 40 |
| 2 | Recruiter portal, jobs, organizations, departments, and matching | `feature/recruiter-jobs` | 30 |
| 3 | Hiring manager, interviews, evaluations, decisions, email, and calendar | `feature/hiring-manager` | 28 |
| 4 | Administrator, users, analytics, audit logs, notifications, and monitoring | `feature/admin-monitoring` | 30 |
| 5 | Authentication, authorization, database, migrations, validation, and core solution setup | `feature/auth-security-database` | 39 |
| 6 | AI feedback, tests, Postman, shared API services, package setup, and technical documentation | `feature/ai-testing-docs` | 43 |

## Required process for every member

1. Accept the GitHub collaborator invitation.
2. Clone the new repository.
3. Configure their own Git username and verified GitHub email.
4. Pull the latest `main` branch.
5. Create their assigned feature branch.
6. Extract their member ZIP outside the cloned repository.
7. Copy the package contents into the repository while preserving the `backend`, `frontend`, and `docs` paths.
8. Review and test the assigned module.
9. Make at least one genuine correction, validation improvement, test, or documentation improvement that they can explain.
10. Commit and push using their own GitHub account.
11. Create a Pull Request into `main`.
12. The repository owner reviews and merges the Pull Request.

## Recommended merge order

1. Member 5 — core setup, authentication, security, and database
2. Member 6 — package configuration, shared services, AI, tests, and documentation
3. Member 1 — candidate portal
4. Member 2 — recruiter and jobs
5. Member 3 — hiring manager workflow
6. Member 4 — administrator and monitoring

The application may not build until all required packages have been merged. Run the final build and tests after all six Pull Requests are merged.

## Evidence to keep

Each member should retain screenshots of their branch, commits, Pull Request, changed files, module pages, and test results. The contribution report should state the files owned, work completed, tests performed, and limitations.

## Academic integrity

The file split establishes module ownership for integration and assessment. Every member should understand, review, test, and be able to explain the code they commit. Git history alone does not prove original authorship.
