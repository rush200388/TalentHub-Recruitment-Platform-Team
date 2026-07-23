using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class CandidateProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? CurrentJobTitle { get; set; }
    public string? ProfessionalSummary { get; set; }
    public int YearsOfExperience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public bool IsProfileComplete { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
