using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Enums;

namespace RecruitmentPlatform.Domain.Entities;

public class Interview : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public string ScheduledByUserId { get; set; } = string.Empty;
    public string? InterviewerUserId { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public InterviewType Type { get; set; } = InterviewType.Online;
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public string? MeetingLink { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    public string? CalendarProvider { get; set; }
    public string? ExternalCalendarEventId { get; set; }

    public ICollection<InterviewFeedback> Feedback { get; set; } =
        new List<InterviewFeedback>();
}
