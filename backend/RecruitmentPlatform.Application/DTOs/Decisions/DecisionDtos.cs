using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Decisions;

public sealed class SaveHiringDecisionRequest
{
    [Required]
    public Guid JobApplicationId { get; set; }

    [Required]
    [StringLength(20)]
    public string Decision { get; set; } =
        string.Empty;

    [StringLength(3000)]
    public string? Notes { get; set; }
}

public sealed record HiringDecisionResponse(
    Guid Id,
    Guid ApplicationId,
    Guid CandidateId,
    string CandidateName,
    Guid JobId,
    string JobTitle,
    string Decision,
    string? Notes,
    string DecidedByUserId,
    string DecidedByName,
    DateTime? DecidedAtUtc,
    decimal? EvaluationScore);
