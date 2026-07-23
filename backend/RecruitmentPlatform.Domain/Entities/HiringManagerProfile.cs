using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class HiringManagerProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Phone { get; set; }
}
