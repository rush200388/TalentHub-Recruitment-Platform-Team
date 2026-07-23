using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Enums;

namespace RecruitmentPlatform.Domain.Entities;

public class HiringDecision : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public string DecidedByUserId { get; set; } = string.Empty;
    public HiringDecisionStatus Status { get; set; } = HiringDecisionStatus.Pending;
    public string? Notes { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
}
