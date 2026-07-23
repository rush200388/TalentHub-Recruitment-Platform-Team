# External Integration Notes

## External AI

`ICandidateFeedbackService` uses the Hybrid Strategy pattern:

```text
HybridCandidateFeedbackService
├── OpenAiCandidateFeedbackClient
└── RuleBasedCandidateFeedbackService
```

When an API key is configured, the backend calls the OpenAI Responses API.
When the service is unavailable or not configured, it falls back to an
explainable rule-based method.

Only de-identified recruitment evidence is sent:

- Job title
- Required and supplied years of experience
- Match score
- Matched and missing skills
- Evaluation score
- Interview score and recommendation

Candidate name, email, phone number, address, and resume text are not sent.

## Cloud storage

`IFileStorageService` uses a Hybrid Storage Strategy:

```text
HybridFileStorageService
├── AzureBlobFileStorageService
└── LocalFileStorageService
```

Azure Blob Storage is used when valid Azure settings exist. Otherwise the
system safely continues with local `App_Data` storage. Existing Phase 4-6
absolute file paths remain readable.

## Honest prototype limitations

- Direct Google Calendar and Microsoft Graph availability checks are not added.
- ICS calendar invitations remain the prototype calendar integration.
- SMS is not added.
- OCR for image-only PDF resumes is not added.
- AI recommendations support human review and do not make final decisions.
