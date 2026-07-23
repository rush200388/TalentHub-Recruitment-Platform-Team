# Member 2 — Recruiter Portal, Jobs, and Matching
GMCDChathuranga
36653

    **Git branch:** `feature/recruiter-jobs`  
    **Assigned source files:** 30

    ## Responsibilities

    - Organizations, departments, skills, and job management
- Recruiter dashboards and candidate review screens
- Hiring-manager directory used by recruiters
- Job-to-candidate matching and ranking logic

    ## How to use this package

    1. Clone the shared repository.
    2. Create the branch `feature/recruiter-jobs`.
    3. Extract/copy the `backend` and `frontend` folders from this package into the cloned repository.
    4. Review and test the assigned module.
    5. Commit only genuine work performed by this member.
    6. Push the branch and create a Pull Request into `main`.

    This package is a module assignment, not a standalone complete application. The complete application is produced after all six branches are merged.

    ## Assigned files

    - `backend/RecruitmentPlatform.Api/Controllers/DepartmentsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/HiringManagersController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/JobsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/OrganizationsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/SkillsController.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Departments/DepartmentDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/HiringManagers/HiringManagerDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Jobs/JobDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Organizations/OrganizationDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Skills/SkillDtos.cs`
- `backend/RecruitmentPlatform.Application/Interfaces/IJobMatchingService.cs`
- `backend/RecruitmentPlatform.Application/Matching/JobMatchResult.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Department.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Job.cs`
- `backend/RecruitmentPlatform.Domain/Entities/JobSkill.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Organization.cs`
- `backend/RecruitmentPlatform.Domain/Entities/RecruiterProfile.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Skill.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721210728_AddJobWorkMode.Designer.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721210728_AddJobWorkMode.cs`
- `backend/RecruitmentPlatform.Infrastructure/Matching/JobMatchingService.cs`
- `backend/SETUP_PHASE3_JOBS.md`
- `frontend/SETUP_FRONTEND_PHASE3_JOBS.md`
- `frontend/src/components/ui/Button.jsx`
- `frontend/src/components/ui/FormField.jsx`
- `frontend/src/components/ui/Modal.jsx`
- `frontend/src/pages/recruiter/CandidatesPage.jsx`
- `frontend/src/pages/recruiter/InterviewSchedulingPage.jsx`
- `frontend/src/pages/recruiter/ManageJobsPage.jsx`
- `frontend/src/pages/recruiter/RecruiterDashboard.jsx`
