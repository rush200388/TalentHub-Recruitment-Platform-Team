using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
