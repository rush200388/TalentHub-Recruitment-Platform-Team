namespace RecruitmentPlatform.Application.ResumeAnalysis;

public sealed record ResumeAnalysisResult(
    int WordCount,
    string? ExtractedEmail,
    string? ExtractedPhone,
    int? SuggestedYearsOfExperience,
    IReadOnlyCollection<string> ExtractedSkills,
    IReadOnlyCollection<string> EducationSignals,
    IReadOnlyCollection<string> ExperienceSignals,
    IReadOnlyCollection<string> Warnings);
