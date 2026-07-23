namespace RecruitmentPlatform.Application.AI;

public sealed record CandidateFeedbackContext(
    string JobTitle,
    int RequiredExperienceYears,
    int CandidateExperienceYears,
    decimal MatchScore,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills,
    decimal? EvaluationScore,
    decimal? InterviewScore,
    string? InterviewRecommendation);
