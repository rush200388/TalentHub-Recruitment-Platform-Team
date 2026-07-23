using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.Validation;

namespace RecruitmentPlatform.Application.DTOs.Candidates;

public sealed record ResumeResponse(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    DateTime UploadedAtUtc,
    bool IsPrimary);

public sealed record CandidateProfileResponse(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    string Name,
    string Email,
    string? Phone,
    string? Location,
    string? CurrentJobTitle,
    string? ProfessionalSummary,
    int YearsOfExperience,
    string? LinkedInUrl,
    string? PortfolioUrl,
    bool IsProfileComplete,
    int CompletenessPercentage,
    IReadOnlyCollection<string> Skills,
    ResumeResponse? Resume);

public sealed record CandidateSummaryResponse(
    Guid Id,
    string UserId,
    string Name,
    string Email,
    string? Phone,
    string? Location,
    string? CurrentJobTitle,
    int YearsOfExperience,
    IReadOnlyCollection<string> Skills,
    int CompletenessPercentage);

public sealed class UpdateCandidateProfileRequest
    : IValidatableObject
{
    [Required]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage =
            "First name must contain 2 to 50 characters.")]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "First name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage =
            "Last name must contain 2 to 50 characters.")]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "Last name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(
        10,
        MinimumLength = 10,
        ErrorMessage =
            "Phone number must contain exactly 10 digits.")]
    [RegularExpression(
        ValidationRules.PhonePattern,
        ErrorMessage =
            "Phone number must contain exactly 10 digits.")]
    public string? Phone { get; set; }

    [StringLength(
        120,
        MinimumLength = 2,
        ErrorMessage =
            "Location must contain 2 to 120 characters.")]
    public string? Location { get; set; }

    [StringLength(
        120,
        MinimumLength = 2,
        ErrorMessage =
            "Professional title must contain 2 to 120 characters.")]
    public string? CurrentJobTitle { get; set; }

    [StringLength(
        2000,
        MinimumLength = 20,
        ErrorMessage =
            "Professional summary must contain 20 to 2000 characters.")]
    public string? ProfessionalSummary { get; set; }

    [Range(
        0,
        60,
        ErrorMessage =
            "Years of experience must be between 0 and 60.")]
    public int YearsOfExperience { get; set; }

    [StringLength(500)]
    public string? LinkedInUrl { get; set; }

    [StringLength(500)]
    public string? PortfolioUrl { get; set; }

    public List<string> Skills { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Phone) &&
            Phone.Length != 10)
        {
            yield return new ValidationResult(
                "Phone number must contain exactly 10 digits.",
                [nameof(Phone)]);
        }

        if (!ValidationRules.IsValidOptionalHttpUrl(
            LinkedInUrl))
        {
            yield return new ValidationResult(
                "LinkedIn URL must begin with http:// or https://.",
                [nameof(LinkedInUrl)]);
        }

        if (!ValidationRules.IsValidOptionalHttpUrl(
            PortfolioUrl))
        {
            yield return new ValidationResult(
                "Portfolio URL must begin with http:// or https://.",
                [nameof(PortfolioUrl)]);
        }

        var normalizedSkills =
            ValidationRules.NormalizeSkills(Skills)
                .ToList();

        if (normalizedSkills.Count > 30)
        {
            yield return new ValidationResult(
                "A profile can contain a maximum of 30 skills.",
                [nameof(Skills)]);
        }

        foreach (var skill in normalizedSkills)
        {
            if (!ValidationRules.IsValidSkill(skill))
            {
                yield return new ValidationResult(
                    $"Skill '{skill}' is invalid. Each skill must contain 1 to 50 valid characters.",
                    [nameof(Skills)]);
            }
        }
    }
}

public sealed record JobRecommendationResponse(
    Guid JobId,
    string Title,
    string Organization,
    string Department,
    string Location,
    string Type,
    string WorkMode,
    string Salary,
    decimal MatchScore,
    string Reason,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills);
