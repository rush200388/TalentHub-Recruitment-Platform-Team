using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.Validation;

namespace RecruitmentPlatform.Application.DTOs.Users;

public sealed record AdminUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Name,
    string Email,
    string? Phone,
    bool IsActive,
    string Status,
    IReadOnlyCollection<string> Roles,
    string PrimaryRole,
    Guid? OrganizationId,
    string Organization,
    Guid? DepartmentId,
    string Department,
    string? JobTitle,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc,
    int FailedLoginAttempts,
    bool IsLockedOut);

public sealed class CreateAdminUserRequest : IValidatableObject
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "First name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "Last name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<string> Roles { get; set; } = [];

    public Guid? OrganizationId { get; set; }
    public Guid? DepartmentId { get; set; }

    [StringLength(120)]
    public string? JobTitle { get; set; }

    [StringLength(10, MinimumLength = 10)]
    [RegularExpression(
        @"^[0-9]{10}$",
        ErrorMessage = "Phone number must contain exactly 10 digits.")]
    public string? Phone { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var organizationRole = Roles.Any(role =>
            string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "HiringManager", StringComparison.OrdinalIgnoreCase));

        if (organizationRole && !OrganizationId.HasValue)
        {
            yield return new ValidationResult(
                "Organization is required for recruiters and hiring managers.",
                [nameof(OrganizationId)]);
        }
    }
}

public sealed class UpdateAdminUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "First name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(
        ValidationRules.PersonNamePattern,
        ErrorMessage =
            "Last name can contain letters, spaces, apostrophes, and hyphens only.")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }
    public Guid? DepartmentId { get; set; }

    [StringLength(120)]
    public string? JobTitle { get; set; }

    [StringLength(10, MinimumLength = 10)]
    [RegularExpression(
        @"^[0-9]{10}$",
        ErrorMessage = "Phone number must contain exactly 10 digits.")]
    public string? Phone { get; set; }
}

public sealed class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class UpdateUserRolesRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    [MaxLength(4)]
    public List<string> Roles { get; set; } = [];

    public Guid? OrganizationId { get; set; }
    public Guid? DepartmentId { get; set; }

    [StringLength(120)]
    public string? JobTitle { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var organizationRole = Roles.Any(role =>
            string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "HiringManager", StringComparison.OrdinalIgnoreCase));

        if (organizationRole && !OrganizationId.HasValue)
        {
            yield return new ValidationResult(
                "Organization is required for recruiters and hiring managers.",
                [nameof(OrganizationId)]);
        }
    }
}
