using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Enums;

namespace RecruitmentPlatform.Domain.Entities;

public class JobApplication : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public string Stage { get; set; } = "Applied";
    public decimal MatchScore { get; set; }
    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    public ICollection<CandidateEvaluation> Evaluations { get; set; } = new List<CandidateEvaluation>();
    public HiringDecision? HiringDecision { get; set; }
}
