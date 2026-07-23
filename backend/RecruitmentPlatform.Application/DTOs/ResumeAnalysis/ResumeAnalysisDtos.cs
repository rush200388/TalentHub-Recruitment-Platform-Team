using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.ResumeAnalysis;

public sealed record ResumeAnalysisResponse(
    Guid ResumeId,
    string FileName,
    string Status,
    string? Strategy,
    DateTime? AnalyzedAtUtc,
    int WordCount,
    string? ExtractedEmail,
    string? ExtractedPhone,
    int? SuggestedYearsOfExperience,
    IReadOnlyCollection<string> ExtractedSkills,
    IReadOnlyCollection<string> EducationSignals,
    IReadOnlyCollection<string> ExperienceSignals,
    IReadOnlyCollection<string> Warnings,
    string? TextPreview);

public sealed class ApplyResumeSkillsRequest
{
    [MaxLength(30)]
    public List<string> Skills { get; set; } = [];
}

public sealed record ApplyResumeSkillsResponse(
    int AddedCount,
    IReadOnlyCollection<string> AddedSkills,
    IReadOnlyCollection<string> CurrentSkills);
