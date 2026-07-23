# Frontend Phase 7

## Install

Stop the frontend and extract this ZIP into:

```text
E:\SA\front end\project
```

Merge folders and replace files.

Run:

```cmd
cd /d "E:\SA\front end\project"
```

```cmd
npm install
```

```cmd
npm run build
```

```cmd
npm run dev
```

## Test AI feedback

Recruiter:

```text
Recruiter → AI Feedback
```

Hiring Manager:

```text
Hiring Manager → AI Feedback
```

Select an application and click:

```text
Generate AI Feedback
```

Without an external API key, the page clearly shows:

```text
Rule-Based Fallback
```

With a configured external AI key, it shows:

```text
External AI
```

The page displays:

- Provider
- Recommendation
- Summary
- Strengths
- Risks and review points
- Suggested candidate-facing feedback
- Fallback reason when applicable
