using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Enums;

namespace RecruitmentPlatform.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
