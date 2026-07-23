using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Analytics;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class AnalyticsController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticsController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            RecruitmentAnalyticsResponse>> Get()
    {
        var applications =
            await _dbContext
                .JobApplications
                .AsNoTracking()
                .Include(x => x.Job)
                    .ThenInclude(x =>
                        x.Department)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.MatchScore,
                    x.AppliedAtUtc,
                    Department =
                        x.Job.Department != null
                            ? x.Job.Department.Name
                            : "General"
                })
                .ToListAsync();

        var decisions =
            await _dbContext
                .HiringDecisions
                .AsNoTracking()
                .Include(x =>
                    x.JobApplication)
                    .ThenInclude(x =>
                        x.Job)
                        .ThenInclude(x =>
                            x.Department)
                .ToListAsync();

        var activeJobs =
            await _dbContext.Jobs
                .CountAsync(x =>
                    x.Status ==
                    JobStatus.Published);

        var scheduledInterviews =
            await _dbContext.Interviews
                .CountAsync(x =>
                    x.Status ==
                    InterviewStatus.Scheduled);

        var completedInterviews =
            await _dbContext.Interviews
                .CountAsync(x =>
                    x.Status ==
                    InterviewStatus.Completed);

        var evaluations =
            await _dbContext
                .CandidateEvaluations
                .AsNoTracking()
                .Select(x =>
                    x.OverallScore)
                .ToListAsync();

        var hired =
            applications.Count(x =>
                x.Status ==
                ApplicationStatus.Hired);

        var rejected =
            applications.Count(x =>
                x.Status ==
                ApplicationStatus.Rejected);

        var shortlisted =
            applications.Count(x =>
                x.Status ==
                    ApplicationStatus
                        .Shortlisted ||
                x.Status ==
                    ApplicationStatus
                        .InterviewScheduled);

        var byDepartment =
            applications
                .GroupBy(x =>
                    x.Department)
                .Select(group =>
                    new DepartmentAnalyticsResponse(
                        group.Key,
                        group.Count(),
                        group.Count(x =>
                            x.Status ==
                            ApplicationStatus
                                .Hired)))
                .OrderByDescending(x =>
                    x.Applications)
                .ToList();

        var monthStarts =
            Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    var date =
                        DateTime.UtcNow
                            .AddMonths(
                                offset - 5);

                    return new DateTime(
                        date.Year,
                        date.Month,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc);
                })
                .ToList();

        var monthlyTrend =
            monthStarts
                .Select(month =>
                    new MonthlyApplicationTrendResponse(
                        month.ToString("MMM"),
                        applications.Count(x =>
                            x.AppliedAtUtc >=
                                month &&
                            x.AppliedAtUtc <
                                month.AddMonths(1))))
                .ToList();

        var approvedDecisions =
            decisions
                .Where(x =>
                    x.Status ==
                        HiringDecisionStatus
                            .Approved &&
                    x.DecidedAtUtc
                        .HasValue)
                .ToList();

        var averageTimeToHire =
            approvedDecisions.Count == 0
                ? 0m
                : Math.Round(
                    (decimal)approvedDecisions
                        .Average(x =>
                            (x.DecidedAtUtc!.Value -
                             x.JobApplication
                                .AppliedAtUtc)
                            .TotalDays),
                    2);

        var averageMatch =
            applications.Count == 0
                ? 0m
                : Math.Round(
                    applications.Average(x =>
                        x.MatchScore),
                    2);

        var averageEvaluation =
            evaluations.Count == 0
                ? 0m
                : Math.Round(
                    evaluations.Average(),
                    2);

        var funnel =
            new List<FunnelStageResponse>
            {
                new(
                    "Applied",
                    applications.Count),
                new(
                    "Under Review",
                    applications.Count(x =>
                        x.Status is
                            ApplicationStatus
                                .Submitted or
                            ApplicationStatus
                                .UnderReview)),
                new(
                    "Shortlisted",
                    applications.Count(x =>
                        x.Status is
                            ApplicationStatus
                                .Shortlisted or
                            ApplicationStatus
                                .InterviewScheduled)),
                new(
                    "Interview",
                    applications.Count(x =>
                        x.Status ==
                            ApplicationStatus
                                .InterviewScheduled)),
                new(
                    "Hired",
                    hired),
                new(
                    "Rejected",
                    rejected)
            };

        return Ok(
            new RecruitmentAnalyticsResponse(
                applications.Count,
                activeJobs,
                shortlisted,
                scheduledInterviews,
                completedInterviews,
                hired,
                rejected,
                averageMatch,
                averageEvaluation,
                averageTimeToHire,
                byDepartment,
                monthlyTrend,
                funnel));
    }
}
