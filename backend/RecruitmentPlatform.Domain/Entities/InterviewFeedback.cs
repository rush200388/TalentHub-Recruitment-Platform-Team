using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class InterviewFeedback : BaseEntity
{
    public Guid InterviewId { get; set; }
    public Interview Interview { get; set; } = null!;

    public string ReviewerUserId { get; set; } = string.Empty;
    public int OverallRating { get; set; }
    public int TechnicalScore { get; set; }
    public int CommunicationScore { get; set; }
    public int CultureFitScore { get; set; }
    public string? Comments { get; set; }
    public string? Recommendation { get; set; }
}
