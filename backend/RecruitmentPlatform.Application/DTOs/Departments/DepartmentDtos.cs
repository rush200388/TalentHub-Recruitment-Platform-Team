using System.ComponentModel.DataAnnotations;

namespace RecruitmentPlatform.Application.DTOs.Departments;

public sealed record DepartmentResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Name,
    string? Description,
    bool IsActive,
    int OpenRoles);

public class CreateDepartmentRequest
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage =
            "Department name must contain 2 to 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateDepartmentRequest
    : CreateDepartmentRequest
{
}
