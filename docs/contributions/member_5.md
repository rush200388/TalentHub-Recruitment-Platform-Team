# Member 5 — Authentication, Security, and Database
## 37048


    **Git branch:** `feature/auth-security-database`  
    **Assigned source files:** 39

    ## Responsibilities

    - JWT authentication and ASP.NET Identity
- Role-based authorization, account lockout, and validation
- PostgreSQL DbContext, seed data, initial migrations, and dependency injection
- Shared authentication frontend and project infrastructure

    ## How to use this package

    1. Clone the shared repository.
    2. Create the branch `feature/auth-security-database`.
    3. Extract/copy the `backend` and `frontend` folders from this package into the cloned repository.
    4. Review and test the assigned module.
    5. Commit only genuine work performed by this member.
    6. Push the branch and create a Pull Request into `main`.

    This package is a module assignment, not a standalone complete application. The complete application is produced after all six branches are merged.

    ## Assigned files

    - `backend/RecruitmentPlatform.Api/Controllers/AuthController.cs`
- `backend/RecruitmentPlatform.Api/Controllers/HealthController.cs`
- `backend/RecruitmentPlatform.Api/Program.cs`
- `backend/RecruitmentPlatform.Api/Properties/launchSettings.json`
- `backend/RecruitmentPlatform.Api/RecruitmentPlatform.Api.csproj`
- `backend/RecruitmentPlatform.Api/RecruitmentPlatform.Api.http`
- `backend/RecruitmentPlatform.Api/WeatherForecast.cs`
- `backend/RecruitmentPlatform.Api/appsettings.Development.json`
- `backend/RecruitmentPlatform.Api/appsettings.json`
- `backend/RecruitmentPlatform.Application/Class1.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Auth/AuthResponse.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Auth/AuthUserResponse.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Auth/LoginRequest.cs`
- `backend/RecruitmentPlatform.Application/DTOs/Auth/RegisterRequest.cs`
- `backend/RecruitmentPlatform.Application/Interfaces/IJwtTokenService.cs`
- `backend/RecruitmentPlatform.Application/RecruitmentPlatform.Application.csproj`
- `backend/RecruitmentPlatform.Application/Security/JwtTokenResult.cs`
- `backend/RecruitmentPlatform.Application/Validation/ValidationRules.cs`
- `backend/RecruitmentPlatform.Domain/Class1.cs`
- `backend/RecruitmentPlatform.Domain/Common/BaseEntity.cs`
- `backend/RecruitmentPlatform.Domain/Enums/RecruitmentEnums.cs`
- `backend/RecruitmentPlatform.Domain/RecruitmentPlatform.Domain.csproj`
- `backend/RecruitmentPlatform.Infrastructure/Authentication/JwtTokenService.cs`
- `backend/RecruitmentPlatform.Infrastructure/Class1.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/ApplicationDbContext.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/DatabaseSeeder.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721201910_InitialCreate.Designer.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/20260721201910_InitialCreate.cs`
- `backend/RecruitmentPlatform.Infrastructure/Data/Migrations/ApplicationDbContextModelSnapshot.cs`
- `backend/RecruitmentPlatform.Infrastructure/DependencyInjection.cs`
- `backend/RecruitmentPlatform.Infrastructure/Identity/ApplicationUser.cs`
- `backend/RecruitmentPlatform.Infrastructure/RecruitmentPlatform.Infrastructure.csproj`
- `backend/RecruitmentPlatform.slnx`
- `frontend/src/components/auth/ProtectedRoute.jsx`
- `frontend/src/context/AuthContext.jsx`
- `frontend/src/pages/auth/LoginPage.jsx`
- `frontend/src/pages/auth/RegisterPage.jsx`
- `frontend/src/services/authService.js`
- `frontend/src/utils/validation.js`
