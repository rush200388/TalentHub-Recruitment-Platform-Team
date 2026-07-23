namespace RecruitmentPlatform.Application.DTOs.AiFeedback;

public sealed record CandidateFeedbackResponse(
    Guid ApplicationId,
    string CandidateName,
    string JobTitle,
    string Provider,
    bool UsedExternalAi,
    string Summary,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<string> Risks,
    string Recommendation,
    string SuggestedFeedback,
    DateTime GeneratedAtUtc,
    string? FallbackReason);
