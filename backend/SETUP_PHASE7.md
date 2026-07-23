# Phase 7 Setup

This phase adds:

- Automated unit tests
- API integration and RBAC tests
- Postman collection and environment
- External AI-generated candidate feedback with safe fallback
- Optional Azure Blob Storage with local fallback
- Cloud-compatible resume download and resume analysis
- Test and integration documentation

## 1. Extract

Stop the backend and extract this ZIP into:

```text
E:\SA\backend
```

Merge folders and replace files.

## 2. Add Azure Blob package

Run from `E:\SA\backend`:

```cmd
dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package Azure.Storage.Blobs
```

## 3. Prepare the test project

Run each command separately:

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj reference RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj
```

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj reference RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj
```

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
```

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite
```

If the test project does not already contain xUnit packages:

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj package Microsoft.NET.Test.Sdk
```

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj package xunit
```

```cmd
dotnet add RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj package xunit.runner.visualstudio
```

## 4. Build and test

```cmd
dotnet clean
```

```cmd
dotnet restore
```

```cmd
dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj
```

```cmd
dotnet test RecruitmentPlatform.Tests\RecruitmentPlatform.Tests.csproj --logger "console;verbosity=normal"
```

No database migration is required for Phase 7.

## 5. External AI configuration

The feature works with rule-based fallback without an API key.

To test the real external provider, run from:

```text
E:\SA\backend\RecruitmentPlatform.Api
```

Set secrets locally. Never push them to GitHub:

```cmd
dotnet user-secrets set "AiProvider:Enabled" "true"
```

```cmd
dotnet user-secrets set "AiProvider:ApiKey" "YOUR_API_KEY"
```

```cmd
dotnet user-secrets set "AiProvider:Model" "gpt-5.6"
```

Do not show the key in screenshots or the demonstration video.

## 6. Azure Blob Storage configuration

Without Azure credentials, the application keeps using local storage.

To enable Azure Blob Storage:

```cmd
dotnet user-secrets set "CloudStorage:Provider" "AzureBlob"
```

```cmd
dotnet user-secrets set "CloudStorage:AzureConnectionString" "YOUR_AZURE_STORAGE_CONNECTION_STRING"
```

```cmd
dotnet user-secrets set "CloudStorage:AzureContainer" "recruitment-files"
```

Upload a new resume after enabling Azure. Existing local resumes continue to work.

## 7. Run

```cmd
dotnet run --project RecruitmentPlatform.Api
```

New endpoint:

```text
POST /api/Applications/{applicationId}/ai-feedback
```

Allowed roles:

```text
Recruiter
HiringManager
Administrator
```

## 8. Postman

Import both files from the `postman` folder:

```text
RecruitmentPlatform_Phase7.postman_collection.json
RecruitmentPlatform_Local.postman_environment.json
```

Enter passwords only in your local Postman collection variables. Do not commit
real passwords.
