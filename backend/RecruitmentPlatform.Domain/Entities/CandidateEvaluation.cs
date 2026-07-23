using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class CandidateEvaluation : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public string EvaluatorUserId { get; set; } = string.Empty;
    public decimal OverallScore { get; set; }
    public decimal SkillsScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal InterviewScore { get; set; }
    public string? Comments { get; set; }
}
