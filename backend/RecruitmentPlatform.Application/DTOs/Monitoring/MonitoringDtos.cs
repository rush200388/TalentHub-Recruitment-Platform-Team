namespace RecruitmentPlatform.Application.DTOs.Monitoring;

public sealed record SystemHealthResponse(
    string Status,
    string Database,
    string Environment,
    string Framework,
    string ApplicationVersion,
    DateTime ProcessStartedAtUtc,
    TimeSpan Uptime,
    DateTime CheckedAtUtc);

public sealed record MonitoringAuditItem(
    Guid Id,
    string User,
    string Action,
    string Entity,
    string Details,
    DateTime TimestampUtc);

public sealed record SystemStatisticsResponse(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int LockedUsers,
    int FailedLoginAttempts,
    int Organizations,
    int Departments,
    int Jobs,
    int Applications,
    int Interviews,
    int StoredResumes,
    int AnalyzedResumes,
    long StoredResumeBytes,
    int AuditLogCount,
    string ResumeStorageProvider,
    IReadOnlyCollection<MonitoringAuditItem> RecentActivity);
