# Frontend Authentication Connection

## 1. Extract

Extract this ZIP into:

E:\SA\front end\project

Allow Windows to merge folders and replace existing files.

## 2. Start the backend

In CMD window 1:

cd /d E:\SA\backend
dotnet run --project RecruitmentPlatform.Api

The expected API URL is:

http://localhost:5117

If the port is different, create a file named .env.local in the
frontend root:

VITE_API_URL=http://localhost:YOUR_PORT/api

## 3. Start the frontend

In CMD window 2:

cd /d "E:\SA\front end\project"
npm install
npm run dev

Open:

http://localhost:5173

## 4. Test

Register a new account.

The frontend should:

- call POST /api/Auth/register
- save the JWT in localStorage
- automatically assign Candidate role
- open the Candidate dashboard

Sign out and sign in again.

The frontend should call:

POST /api/Auth/login

Refreshing the page calls:

GET /api/Auth/me

## Important

The remaining jobs, candidates, applications, interviews and admin
pages still use mock data until their backend endpoints are implemented
in later phases.
