using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.Validation;

namespace RecruitmentPlatform.Application.DTOs.Organizations;

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Website,
    bool IsActive,
    int DepartmentCount,
    int JobCount);

public class CreateOrganizationRequest
    : IValidatableObject
{
    [Required]
    [StringLength(
        150,
        MinimumLength = 2,
        ErrorMessage =
            "Organization name must contain 2 to 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!ValidationRules.IsValidOptionalHttpUrl(
            Website))
        {
            yield return new ValidationResult(
                "Website must begin with http:// or https://.",
                [nameof(Website)]);
        }
    }
}

public sealed class UpdateOrganizationRequest
    : CreateOrganizationRequest
{
}
