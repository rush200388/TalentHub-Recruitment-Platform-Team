using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Applications;

public sealed class CreateApplicationRequest
{
    [Required]
    public Guid JobId { get; set; }

    [StringLength(
        3000,
        ErrorMessage =
            "Cover letter cannot exceed 3000 characters.")]
    public string? CoverLetter { get; set; }
}

public sealed class UpdateApplicationStatusRequest
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Stage { get; set; }
}

public sealed record ApplicationResponse(
    Guid Id,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string? CandidatePhone,
    string? CandidateTitle,
    string? CandidateLocation,
    int CandidateExperience,
    IReadOnlyCollection<string> CandidateSkills,
    Guid JobId,
    string JobTitle,
    string Organization,
    string Department,
    DateTime AppliedAtUtc,
    string AppliedOn,
    string Status,
    string Stage,
    decimal MatchScore,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills,
    string MatchReason,
    string? CoverLetter);
