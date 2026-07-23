using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Enums;

namespace RecruitmentPlatform.Domain.Entities;

public class Job : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? RecruiterProfileId { get; set; }
    public RecruiterProfile? RecruiterProfile { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string Location { get; set; } = string.Empty;
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public WorkMode WorkMode { get; set; } = WorkMode.OnSite;
    public int MinimumExperienceYears { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public string Currency { get; set; } = "USD";
    public JobStatus Status { get; set; } = JobStatus.Draft;
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ClosingAtUtc { get; set; }

    public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
