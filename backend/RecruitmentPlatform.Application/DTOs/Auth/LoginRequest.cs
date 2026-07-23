using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress(
        ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}
