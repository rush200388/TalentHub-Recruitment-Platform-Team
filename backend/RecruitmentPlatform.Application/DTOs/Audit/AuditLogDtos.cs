namespace RecruitmentPlatform.Application.DTOs.Audit;

public sealed record AuditLogResponse(
    Guid Id,
    DateTime TimestampUtc,
    string Timestamp,
    string User,
    string Action,
    string Target,
    string Detail,
    string? IpAddress);
