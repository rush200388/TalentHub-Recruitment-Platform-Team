namespace RecruitmentPlatform.Application.Matching;

public sealed record JobMatchResult(
    decimal Score,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills,
    string Reason);
