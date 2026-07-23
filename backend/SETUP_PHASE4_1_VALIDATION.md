# Phase 4.1 — Validation and Data Quality Fix

This patch adds both server-side and client-side validation.

Server-side validation is the final security boundary. Invalid values return
HTTP 400 and are not saved to PostgreSQL.

Main limits:

- Names: 2–50 characters; letters, spaces, apostrophes, hyphens
- Phone: 7–15 digits
- Experience: 0–60 years
- Job title: 3–120 characters
- Location: 2–120 characters
- Description: 30–5000 characters
- Salary: 0–1,000,000,000
- Currency: exactly 3 letters
- Closing date: future dates only
- Skills: maximum 30, each maximum 50 characters
- Cover letter: maximum 3000 characters
- URLs: http:// or https:// only
- Resume: existing PDF/DOCX and 5 MB checks remain

No migration is required because this patch changes request validation only.
