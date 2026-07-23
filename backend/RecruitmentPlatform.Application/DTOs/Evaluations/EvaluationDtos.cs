using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Evaluations;

public sealed class SaveCandidateEvaluationRequest
{
    [Required]
    public Guid JobApplicationId { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "100",
        ErrorMessage =
            "Skills score must be between 0 and 100.")]
    public decimal SkillsScore { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "100",
        ErrorMessage =
            "Experience score must be between 0 and 100.")]
    public decimal ExperienceScore { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "100",
        ErrorMessage =
            "Interview score must be between 0 and 100.")]
    public decimal InterviewScore { get; set; }

    [StringLength(3000)]
    public string? Comments { get; set; }
}

public sealed record CandidateEvaluationResponse(
    Guid Id,
    Guid ApplicationId,
    Guid CandidateId,
    string CandidateName,
    Guid JobId,
    string JobTitle,
    string EvaluatorUserId,
    string EvaluatorName,
    decimal SkillsScore,
    decimal ExperienceScore,
    decimal InterviewScore,
    decimal OverallScore,
    string? Comments,
    DateTime UpdatedAtUtc);
