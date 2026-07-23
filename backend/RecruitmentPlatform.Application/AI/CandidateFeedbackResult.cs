namespace RecruitmentPlatform.Application.AI;

public sealed record CandidateFeedbackResult(
    string Provider,
    bool UsedExternalAi,
    string Summary,
    IReadOnlyCollection<string> Strengths,
    IReadOnlyCollection<string> Risks,
    string Recommendation,
    string SuggestedFeedback,
    DateTime GeneratedAtUtc,
    string? FallbackReason);
