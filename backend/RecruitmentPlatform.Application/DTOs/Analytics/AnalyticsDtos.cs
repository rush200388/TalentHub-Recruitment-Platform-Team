namespace RecruitmentPlatform.Application.DTOs.Analytics;

public sealed record DepartmentAnalyticsResponse(
    string Department,
    int Applications,
    int Hires);

public sealed record MonthlyApplicationTrendResponse(
    string Month,
    int Applications);

public sealed record FunnelStageResponse(
    string Stage,
    int Count);

public sealed record RecruitmentAnalyticsResponse(
    int TotalApplications,
    int ActiveJobs,
    int Shortlisted,
    int ScheduledInterviews,
    int CompletedInterviews,
    int Hired,
    int Rejected,
    decimal AverageMatchScore,
    decimal AverageEvaluationScore,
    decimal AverageTimeToHireDays,
    IReadOnlyCollection<DepartmentAnalyticsResponse>
        ByDepartment,
    IReadOnlyCollection<MonthlyApplicationTrendResponse>
        MonthlyTrend,
    IReadOnlyCollection<FunnelStageResponse>
        Funnel);
