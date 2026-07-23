# Recruitment Platform Backend — Phase 1

This package adds:

- PostgreSQL + Entity Framework Core
- ASP.NET Core Identity users and roles
- Recruitment database entities
- ApplicationDbContext
- Role and optional administrator seeding
- JWT validation configuration
- Swagger JWT support
- React development CORS policy

## 1. Copy files

Extract this ZIP into:

E:\SA\backend

Allow Windows to merge the project folders.

This intentionally replaces:

- RecruitmentPlatform.Api\Program.cs
- RecruitmentPlatform.Api\appsettings.json

Delete the default template files if present:

- RecruitmentPlatform.Api\Controllers\WeatherForecastController.cs
- RecruitmentPlatform.Api\WeatherForecast.cs

## 2. Install packages

From E:\SA\backend:

dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore

dotnet add RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj package Swashbuckle.AspNetCore

## 3. Save local secrets

From E:\SA\backend\RecruitmentPlatform.Api:

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RecruitmentPlatformDb;Username=postgres;Password=YOUR_PASSWORD"

dotnet user-secrets set "Jwt:Key" "CHANGE-THIS-TO-A-LONG-RANDOM-LOCAL-DEVELOPMENT-KEY-2026"

Optional administrator:

dotnet user-secrets set "SeedAdmin:Email" "admin@recruitment.local"
dotnet user-secrets set "SeedAdmin:Password" "Admin12345"

## 4. Build

From E:\SA\backend:

dotnet restore
dotnet build

## 5. Create the migration and database tables

dotnet ef migrations add InitialCreate --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api --output-dir Data\Migrations

dotnet ef database update --project RecruitmentPlatform.Infrastructure --startup-project RecruitmentPlatform.Api

## 6. Start the API

dotnet run --project RecruitmentPlatform.Api

Open the Swagger URL printed in the terminal.

Phase 2 will add register/login JWT endpoints and connect the React login and registration pages.
