using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class Department : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<RecruiterProfile> Recruiters { get; set; } = new List<RecruiterProfile>();
    public ICollection<HiringManagerProfile> HiringManagers { get; set; } = new List<HiringManagerProfile>();
}
