# Phase 2 — Authentication API

## Why Swagger previously said "No operations defined"

Swagger only lists public API actions discovered from controller classes. Phase 1 had
database and security configuration but no application controllers.

## 1. Extract

Extract this ZIP into:

E:\SA\backend

Allow Windows to merge folders and replace:

- RecruitmentPlatform.Infrastructure\DependencyInjection.cs
- RecruitmentPlatform.Api\Program.cs

## 2. Add JWT token-generation package

Run from E:\SA\backend:

dotnet add RecruitmentPlatform.Infrastructure\RecruitmentPlatform.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt

## 3. Build

dotnet clean
dotnet build RecruitmentPlatform.Api\RecruitmentPlatform.Api.csproj

## 4. Run

dotnet run --project RecruitmentPlatform.Api

Open:

http://localhost:5117/swagger

Use the exact port shown in the terminal if it changes.

## 5. Test registration

Open POST /api/Auth/register and use:

{
  "firstName": "Test",
  "lastName": "Candidate",
  "email": "candidate@test.com",
  "password": "Candidate123"
}

This public endpoint always creates a Candidate account. Recruiter, hiring-manager,
and administrator accounts must not be selectable during public registration.

## 6. Test login

Open POST /api/Auth/login:

{
  "email": "candidate@test.com",
  "password": "Candidate123"
}

Copy only the token value from the response.

## 7. Test protected current-user endpoint

Click Authorize in Swagger and paste only the token value. Then call:

GET /api/Auth/me

## 8. Health check

Call:

GET /api/Health

It should return:

{
  "status": "Healthy",
  "database": "Connected"
}

No new migration is needed for this phase because it uses the existing Identity and
CandidateProfiles tables.
