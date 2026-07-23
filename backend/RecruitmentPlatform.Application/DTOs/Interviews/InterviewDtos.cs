using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Interviews;

public sealed class CreateInterviewRequest
    : IValidatableObject
{
    [Required]
    public Guid JobApplicationId { get; set; }

    [Required]
    [StringLength(450)]
    public string InterviewerUserId { get; set; } =
        string.Empty;

    [Required]
    public DateTime StartTimeUtc { get; set; }

    [Required]
    public DateTime EndTimeUtc { get; set; }

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Online";

    [StringLength(500)]
    public string? MeetingLink { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(3000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartTimeUtc <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Interview start time must be in the future.",
                [nameof(StartTimeUtc)]);
        }

        if (EndTimeUtc <= StartTimeUtc)
        {
            yield return new ValidationResult(
                "Interview end time must be after the start time.",
                [nameof(EndTimeUtc)]);
        }

        if (EndTimeUtc - StartTimeUtc >
            TimeSpan.FromHours(8))
        {
            yield return new ValidationResult(
                "An interview cannot be longer than 8 hours.",
                [nameof(EndTimeUtc)]);
        }
    }
}

public sealed class UpdateInterviewRequest
    : IValidatableObject
{
    [Required]
    [StringLength(450)]
    public string InterviewerUserId { get; set; } =
        string.Empty;

    [Required]
    public DateTime StartTimeUtc { get; set; }

    [Required]
    public DateTime EndTimeUtc { get; set; }

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = "Online";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    [StringLength(500)]
    public string? MeetingLink { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(3000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.Equals(
                Status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase) &&
            StartTimeUtc <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Interview start time must be in the future.",
                [nameof(StartTimeUtc)]);
        }

        if (EndTimeUtc <= StartTimeUtc)
        {
            yield return new ValidationResult(
                "Interview end time must be after the start time.",
                [nameof(EndTimeUtc)]);
        }

        if (EndTimeUtc - StartTimeUtc >
            TimeSpan.FromHours(8))
        {
            yield return new ValidationResult(
                "An interview cannot be longer than 8 hours.",
                [nameof(EndTimeUtc)]);
        }
    }
}

public sealed class SubmitInterviewFeedbackRequest
{
    [Range(
        1,
        5,
        ErrorMessage =
            "Overall rating must be between 1 and 5.")]
    public int OverallRating { get; set; }

    [Range(
        1,
        5,
        ErrorMessage =
            "Technical score must be between 1 and 5.")]
    public int TechnicalScore { get; set; }

    [Range(
        1,
        5,
        ErrorMessage =
            "Communication score must be between 1 and 5.")]
    public int CommunicationScore { get; set; }

    [Range(
        1,
        5,
        ErrorMessage =
            "Culture-fit score must be between 1 and 5.")]
    public int CultureFitScore { get; set; }

    [StringLength(3000)]
    public string? Comments { get; set; }

    [Required]
    [StringLength(30)]
    public string Recommendation { get; set; } =
        string.Empty;
}

public sealed record InterviewFeedbackResponse(
    Guid Id,
    string ReviewerUserId,
    string ReviewerName,
    int OverallRating,
    int TechnicalScore,
    int CommunicationScore,
    int CultureFitScore,
    string? Comments,
    string Recommendation,
    DateTime SubmittedAtUtc);

public sealed record InterviewResponse(
    Guid Id,
    Guid ApplicationId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    Guid JobId,
    string JobTitle,
    string Organization,
    string InterviewerUserId,
    string InterviewerName,
    string InterviewerEmail,
    string ScheduledByUserId,
    string ScheduledByName,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string Date,
    string Time,
    string EndTime,
    string Type,
    string Status,
    string? MeetingLink,
    string? Location,
    string? Notes,
    InterviewFeedbackResponse? Feedback);
