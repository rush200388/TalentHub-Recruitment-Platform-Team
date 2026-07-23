using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.Validation;

namespace RecruitmentPlatform.Application.DTOs.Auth;

public sealed class RegisterRequest
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

    [Required]
    [EmailAddress(
        ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage =
            "Password must contain 8 to 100 characters.")]
    public string Password { get; set; } = string.Empty;
}
