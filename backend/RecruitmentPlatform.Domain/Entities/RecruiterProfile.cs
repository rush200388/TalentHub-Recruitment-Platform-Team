using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class RecruiterProfile : BaseEntity
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

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
