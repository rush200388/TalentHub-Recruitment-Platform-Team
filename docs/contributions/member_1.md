# Member 1 — Candidate Portal and Resume Management

    **Git branch:** `feature/candidate-portal`  
    **Assigned source files:** 40

    ## Responsibilities

    - Candidate profile management
- Resume upload, storage, download, parsing, and skill extraction
- Job browsing, recommendations, applications, and status tracking
- Candidate-side validation and resume-analysis UI

    ## How to use this package

    1. Clone the shared repository.
    2. Create the branch `feature/candidate-portal`.
    3. Extract/copy the `backend` and `frontend` folders from this package into the cloned repository.
    4. Review and test the assigned module.
    5. Commit only genuine work performed by this member.
    6. Push the branch and create a Pull Request into `main`.

    This package is a module assignment, not a standalone complete application. The complete application is produced after all six branches are merged.

    ## Assigned files

    - `backend/PHONE_FIX_README.md`
- `backend/RecruitmentPlatform.Api/Controllers/ApplicationsController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/CandidatesController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/ResumeAnalysisController.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Applications/ApplicationDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Candidates/CandidateDtos.cs`
- `backend/RecruitmentPlatform.Application/DTOs/ResumeAnalysis/ResumeAnalysisDtos.cs`
- `backend/RecruitmentPlatform.Application/Interfaces/IFileStorageService.cs`
- `backend/RecruitmentPlatform.Application/Interfaces/IResumeAnalysisService.cs`
- `backend/RecruitmentPlatform.Application/Interfaces/IResumeTextExtractionService.cs`
- `backend/RecruitmentPlatform.Application/ResumeAnalysis/ResumeAnalysisResult.cs`
- `backend/RecruitmentPlatform.Application/Storage/StoredFileResult.cs`
- `backend/RecruitmentPlatform.Domain/Entities/CandidateProfile.cs`
- `backend/RecruitmentPlatform.Domain/Entities/CandidateSkill.cs`
- `backend/RecruitmentPlatform.Domain/Entities/JobApplication.cs`
- `backend/RecruitmentPlatform.Domain/Entities/Resume.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721223647_AddResumeAnalysis.Designer.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721223647_AddResumeAnalysis.cs`
- `backend/RecruitmentPlatform.Infrastructure/ResumeAnalysis/ResumeTextExtractionService.cs`
- `backend/RecruitmentPlatform.Infrastructure/ResumeAnalysis/RuleBasedResumeAnalysisService.cs`
- `backend/RecruitmentPlatform.Infrastructure/Storage/AzureBlobFileStorageService.cs`
- `backend/RecruitmentPlatform.Infrastructure/Storage/CloudStorageOptions.cs`
- `backend/RecruitmentPlatform.Infrastructure/Storage/HybridFileStorageService.cs`
- `backend/RecruitmentPlatform.Infrastructure/Storage/LocalFileStorageService.cs`
- `backend/SETUP_PHASE4.md`
- `backend/SETUP_PHASE4_1_VALIDATION.md`
- `frontend/PHONE_FIX_README.md`
- `frontend/SETUP_FRONTEND_PHASE4.md`
- `frontend/SETUP_FRONTEND_PHASE4_1_VALIDATION.md`
- `frontend/src/components/jobs/JobCard.jsx`
- `frontend/src/components/ui/Alert.jsx`
- `frontend/src/components/ui/ScoreBar.jsx`
- `frontend/src/components/ui/Spinner.jsx`
- `frontend/src/pages/candidate/BrowseJobsPage.jsx`
- `frontend/src/pages/candidate/CandidateDashboard.jsx`
- `frontend/src/pages/candidate/CandidateProfilePage.jsx`
- `frontend/src/pages/candidate/JobDetailsPage.jsx`
- `frontend/src/pages/candidate/MyApplicationsPage.jsx`
- `frontend/src/pages/candidate/RecommendationsPage.jsx`
- `frontend/src/pages/candidate/ResumeAnalysisPage.jsx`
