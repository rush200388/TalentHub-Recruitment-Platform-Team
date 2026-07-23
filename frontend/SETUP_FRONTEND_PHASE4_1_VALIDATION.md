# Frontend Phase 4.1 — Validation Fix

This patch adds visible validation to registration, candidate profile,
job management, organizations, departments, and application cover letters.

Important: HTML min/max values do not completely prevent a user from typing
an invalid value. The form now displays errors before sending, and the API
independently rejects invalid data with HTTP 400.

No npm package and no database migration are required.
