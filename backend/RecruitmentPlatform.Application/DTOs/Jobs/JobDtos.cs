using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.Validation;

namespace RecruitmentPlatform.Application.DTOs.Jobs;

public sealed record JobResponse(
    Guid Id,
    Guid OrganizationId,
    string Organization,
    Guid? DepartmentId,
    string Department,
    string Recruiter,
    string Title,
    string Description,
    string? Responsibilities,
    string? Requirements,
    string Location,
    string Type,
    string Remote,
    int MinimumExperienceYears,
    string Experience,
    decimal? MinimumSalary,
    decimal? MaximumSalary,
    string Currency,
    string Salary,
    string Status,
    DateTime? PublishedAtUtc,
    DateTime? ClosingAtUtc,
    string Posted,
    IReadOnlyCollection<string> Skills);

public sealed class SaveJobRequest
    : IValidatableObject
{
    public Guid? OrganizationId { get; set; }

    public Guid? DepartmentId { get; set; }

    [Required]
    [StringLength(
        120,
        MinimumLength = 3,
        ErrorMessage =
            "Job title must contain 3 to 120 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(
        5000,
        MinimumLength = 30,
        ErrorMessage =
            "Description must contain 30 to 5000 characters.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(
        5000,
        MinimumLength = 10,
        ErrorMessage =
            "Responsibilities must contain 10 to 5000 characters.")]
    public string? Responsibilities { get; set; }

    [StringLength(
        5000,
        MinimumLength = 10,
        ErrorMessage =
            "Requirements must contain 10 to 5000 characters.")]
    public string? Requirements { get; set; }

    [Required]
    [StringLength(
        120,
        MinimumLength = 2,
        ErrorMessage =
            "Location must contain 2 to 120 characters.")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string EmploymentType { get; set; } = "Full-time";

    [Required]
    [StringLength(20)]
    public string WorkMode { get; set; } = "On-site";

    [Range(
        0,
        60,
        ErrorMessage =
            "Minimum experience must be between 0 and 60 years.")]
    public int MinimumExperienceYears { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage =
            "Minimum salary must be between 0 and 1,000,000,000.")]
    public decimal? MinimumSalary { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "1000000000",
        ErrorMessage =
            "Maximum salary must be between 0 and 1,000,000,000.")]
    public decimal? MaximumSalary { get; set; }

    [Required]
    [RegularExpression(
        ValidationRules.CurrencyPattern,
        ErrorMessage =
            "Currency must contain exactly three letters, such as LKR or USD.")]
    public string Currency { get; set; } = "USD";

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    public DateTime? ClosingAtUtc { get; set; }

    public List<string> Skills { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MinimumSalary.HasValue &&
            MaximumSalary.HasValue &&
            MinimumSalary.Value >
            MaximumSalary.Value)
        {
            yield return new ValidationResult(
                "Minimum salary cannot be greater than maximum salary.",
                [nameof(MaximumSalary)]);
        }

        if (ClosingAtUtc.HasValue &&
            ClosingAtUtc.Value <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Closing date must be in the future.",
                [nameof(ClosingAtUtc)]);
        }

        var normalizedSkills =
            ValidationRules.NormalizeSkills(Skills)
                .ToList();

        if (normalizedSkills.Count > 30)
        {
            yield return new ValidationResult(
                "A job can contain a maximum of 30 skills.",
                [nameof(Skills)]);
        }

        var published =
            string.Equals(
                Status,
                "Open",
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                Status,
                "Published",
                StringComparison.OrdinalIgnoreCase);

        if (published && normalizedSkills.Count == 0)
        {
            yield return new ValidationResult(
                "A published job must include at least one required skill.",
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
