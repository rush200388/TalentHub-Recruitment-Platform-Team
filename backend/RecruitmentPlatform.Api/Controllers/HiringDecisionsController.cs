using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Decisions;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles =
    "Recruiter,HiringManager,Administrator")]
public sealed class HiringDecisionsController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HiringDecisionsController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                HiringDecisionResponse>>> List()
    {
        var query =
            DecisionQuery()
                .AsNoTracking()
                .AsQueryable();

        if (User.IsInRole("Recruiter"))
        {
            var userId =
                GetCurrentUserId();

            query = query.Where(x =>
                x.JobApplication.Job
                    .RecruiterProfile != null &&
                x.JobApplication.Job
                    .RecruiterProfile.UserId ==
                userId);
        }
        else if (User.IsInRole(
            "HiringManager"))
        {
            var access =
                await GetManagerAccess();

            if (access is null)
            {
                return Ok(
                    Array.Empty<
                        HiringDecisionResponse>());
            }

            query = query.Where(x =>
                x.JobApplication.Job
                    .OrganizationId ==
                    access.Value.OrganizationId &&
                (!access.Value.DepartmentId
                    .HasValue ||
                 x.JobApplication.Job
                    .DepartmentId ==
                    access.Value.DepartmentId));
        }

        var decisions =
            await query
                .OrderByDescending(x =>
                    x.DecidedAtUtc ??
                    x.CreatedAtUtc)
                .ToListAsync();

        return Ok(
            await ToResponses(decisions));
    }

    [HttpPost]
    [Authorize(Roles =
        "HiringManager,Administrator")]
    public async Task<
        ActionResult<
            HiringDecisionResponse>> Save(
            SaveHiringDecisionRequest request)
    {
        var application =
            await ApplicationQuery()
                .SingleOrDefaultAsync(x =>
                    x.Id ==
                    request.JobApplicationId);

        if (application is null)
        {
            return NotFound(new
            {
                message =
                    "Application not found."
            });
        }

        if (!await CanManagerAccess(
            application))
        {
            return Forbid();
        }

        if (application.Status is not (
            ApplicationStatus.Shortlisted or
            ApplicationStatus.InterviewScheduled or
            ApplicationStatus.UnderReview or
            ApplicationStatus.Offered or
            ApplicationStatus.Hired or
            ApplicationStatus.Rejected))
        {
            return BadRequest(new
            {
                message =
                    "This application is not ready for a hiring decision."
            });
        }

        if (!TryParseDecision(
            request.Decision,
            out var decisionStatus,
            out var applicationStatus,
            out var stage,
            out var displayDecision))
        {
            return BadRequest(new
            {
                message =
                    "Decision must be Hire, Reject, or On Hold."
            });
        }

        var userId =
            GetCurrentUserId();

        var decision =
            await _dbContext
                .HiringDecisions
                .SingleOrDefaultAsync(x =>
                    x.JobApplicationId ==
                    application.Id);

        if (decision is null)
        {
            decision =
                new HiringDecision
                {
                    JobApplicationId =
                        application.Id
                };

            _dbContext
                .HiringDecisions
                .Add(decision);
        }

        decision.DecidedByUserId =
            userId;
        decision.Status =
            decisionStatus;
        decision.Notes =
            Clean(request.Notes);
        decision.DecidedAtUtc =
            DateTime.UtcNow;

        application.Status =
            applicationStatus;
        application.Stage = stage;

        _dbContext.Notifications.Add(
            new Notification
            {
                UserId =
                    application
                        .CandidateProfile
                        .UserId,
                Type =
                    NotificationType
                        .ApplicationUpdate,
                Title =
                    "Hiring decision updated",
                Message =
                    $"The decision for your application to {application.Job.Title} is {displayDecision}."
            });

        AddAudit(
            "HiringDecision",
            "HiringDecision",
            decision.Id,
            $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName} - {displayDecision}");

        await _dbContext.SaveChangesAsync();

        var saved =
            await DecisionQuery()
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id ==
                    decision.Id);

        return Ok(
            (await ToResponses([saved]))
                .Single());
    }

    private IQueryable<HiringDecision>
        DecisionQuery()
    {
        return _dbContext
            .HiringDecisions
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.CandidateProfile)
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.Job)
                    .ThenInclude(x =>
                        x.RecruiterProfile)
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.Evaluations);
    }

    private IQueryable<JobApplication>
        ApplicationQuery()
    {
        return _dbContext
            .JobApplications
            .Include(x =>
                x.CandidateProfile)
            .Include(x =>
                x.Job)
                .ThenInclude(x =>
                    x.RecruiterProfile);
    }

    private async Task<bool>
        CanManagerAccess(
            JobApplication application)
    {
        if (User.IsInRole(
            "Administrator"))
        {
            return true;
        }

        if (!User.IsInRole(
            "HiringManager"))
        {
            return false;
        }

        var access =
            await GetManagerAccess();

        return access is not null &&
            application.Job.OrganizationId ==
                access.Value.OrganizationId &&
            (!access.Value.DepartmentId
                .HasValue ||
             application.Job.DepartmentId ==
                access.Value.DepartmentId);
    }

    private async Task<
        (Guid OrganizationId,
        Guid? DepartmentId)?>
        GetManagerAccess()
    {
        var profile =
            await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId ==
                    GetCurrentUserId());

        if (profile?.OrganizationId
            is null)
        {
            return null;
        }

        return (
            profile.OrganizationId.Value,
            profile.DepartmentId);
    }

    private async Task<
        IReadOnlyCollection<
            HiringDecisionResponse>>
        ToResponses(
            IReadOnlyCollection<
                HiringDecision> decisions)
    {
        var userIds =
            decisions
                .Select(x =>
                    x.DecidedByUserId)
                .Distinct()
                .ToArray();

        var users =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x =>
                        $"{x.FirstName} {x.LastName}"
                            .Trim());

        return decisions
            .Select(x =>
            {
                var latestEvaluation =
                    x.JobApplication
                        .Evaluations
                        .OrderByDescending(e =>
                            e.UpdatedAtUtc ??
                            e.CreatedAtUtc)
                        .FirstOrDefault();

                return new HiringDecisionResponse(
                    x.Id,
                    x.JobApplicationId,
                    x.JobApplication
                        .CandidateProfileId,
                    $"{x.JobApplication.CandidateProfile.FirstName} {x.JobApplication.CandidateProfile.LastName}"
                        .Trim(),
                    x.JobApplication.JobId,
                    x.JobApplication.Job.Title,
                    DisplayDecision(
                        x.Status),
                    x.Notes,
                    x.DecidedByUserId,
                    users.GetValueOrDefault(
                        x.DecidedByUserId,
                        "Unknown user"),
                    x.DecidedAtUtc,
                    latestEvaluation
                        ?.OverallScore);
            })
            .ToList();
    }

    private static bool TryParseDecision(
        string value,
        out HiringDecisionStatus
            decisionStatus,
        out ApplicationStatus
            applicationStatus,
        out string stage,
        out string displayDecision)
    {
        var normalized =
            value.Trim()
                .Replace(
                    " ",
                    string.Empty)
                .ToLowerInvariant();

        switch (normalized)
        {
            case "hire":
            case "hired":
            case "approved":
                decisionStatus =
                    HiringDecisionStatus
                        .Approved;
                applicationStatus =
                    ApplicationStatus.Hired;
                stage = "Hired";
                displayDecision = "Hired";
                return true;

            case "reject":
            case "rejected":
                decisionStatus =
                    HiringDecisionStatus
                        .Rejected;
                applicationStatus =
                    ApplicationStatus
                        .Rejected;
                stage = "Closed";
                displayDecision =
                    "Rejected";
                return true;

            case "onhold":
            case "hold":
                decisionStatus =
                    HiringDecisionStatus
                        .OnHold;
                applicationStatus =
                    ApplicationStatus
                        .UnderReview;
                stage = "On Hold";
                displayDecision =
                    "On Hold";
                return true;

            default:
                decisionStatus = default;
                applicationStatus = default;
                stage = string.Empty;
                displayDecision =
                    string.Empty;
                return false;
        }
    }

    private static string DisplayDecision(
        HiringDecisionStatus status)
    {
        return status switch
        {
            HiringDecisionStatus
                .Approved => "Hired",
            HiringDecisionStatus
                .Rejected => "Rejected",
            HiringDecisionStatus
                .OnHold => "On Hold",
            _ => "Pending"
        };
    }

    private static string? Clean(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? throw new
                UnauthorizedAccessException();
    }

    private void AddAudit(
        string action,
        string entityName,
        Guid entityId,
        string details)
    {
        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId =
                    GetCurrentUserId(),
                Action = action,
                EntityName =
                    entityName,
                EntityId =
                    entityId.ToString(),
                Details = details,
                IpAddress =
                    HttpContext.Connection
                        .RemoteIpAddress
                        ?.ToString()
            });
    }
}
