using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Evaluations;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles =
    "Recruiter,HiringManager,Administrator")]
public sealed class EvaluationsController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public EvaluationsController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                CandidateEvaluationResponse>>> List(
            [FromQuery] Guid? applicationId)
    {
        var query =
            EvaluationQuery()
                .AsNoTracking()
                .AsQueryable();

        if (applicationId.HasValue)
        {
            query = query.Where(x =>
                x.JobApplicationId ==
                    applicationId.Value);
        }

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
                        CandidateEvaluationResponse>());
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

        var evaluations =
            await query
                .OrderByDescending(x =>
                    x.UpdatedAtUtc ??
                    x.CreatedAtUtc)
                .ToListAsync();

        return Ok(
            await ToResponses(evaluations));
    }

    [HttpPost]
    [Authorize(Roles =
        "HiringManager,Administrator")]
    public async Task<
        ActionResult<
            CandidateEvaluationResponse>> Save(
            SaveCandidateEvaluationRequest request)
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
            RecruitmentPlatform.Domain.Enums.ApplicationStatus.Shortlisted or
            RecruitmentPlatform.Domain.Enums.ApplicationStatus.InterviewScheduled or
            RecruitmentPlatform.Domain.Enums.ApplicationStatus.UnderReview or
            RecruitmentPlatform.Domain.Enums.ApplicationStatus.Offered))
        {
            return BadRequest(new
            {
                message =
                    "Only shortlisted or interview-stage candidates can be evaluated."
            });
        }

        var userId =
            GetCurrentUserId();

        var evaluation =
            await _dbContext
                .CandidateEvaluations
                .SingleOrDefaultAsync(x =>
                    x.JobApplicationId ==
                        application.Id &&
                    x.EvaluatorUserId ==
                        userId);

        if (evaluation is null)
        {
            evaluation =
                new CandidateEvaluation
                {
                    JobApplicationId =
                        application.Id,
                    EvaluatorUserId =
                        userId
                };

            _dbContext
                .CandidateEvaluations
                .Add(evaluation);
        }

        evaluation.SkillsScore =
            request.SkillsScore;
        evaluation.ExperienceScore =
            request.ExperienceScore;
        evaluation.InterviewScore =
            request.InterviewScore;
        evaluation.OverallScore =
            CalculateOverall(
                request.SkillsScore,
                request.ExperienceScore,
                request.InterviewScore);
        evaluation.Comments =
            Clean(request.Comments);

        AddAudit(
            "Evaluate",
            "CandidateEvaluation",
            evaluation.Id,
            $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName} - {application.Job.Title}");

        await _dbContext.SaveChangesAsync();

        var saved =
            await EvaluationQuery()
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id ==
                    evaluation.Id);

        return Ok(
            (await ToResponses([saved]))
                .Single());
    }

    private IQueryable<
        CandidateEvaluation>
        EvaluationQuery()
    {
        return _dbContext
            .CandidateEvaluations
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.CandidateProfile)
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.Job)
                    .ThenInclude(x =>
                        x.RecruiterProfile);
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
            CandidateEvaluationResponse>>
        ToResponses(
            IReadOnlyCollection<
                CandidateEvaluation>
                    evaluations)
    {
        var userIds =
            evaluations
                .Select(x =>
                    x.EvaluatorUserId)
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

        return evaluations
            .Select(x =>
                new CandidateEvaluationResponse(
                    x.Id,
                    x.JobApplicationId,
                    x.JobApplication
                        .CandidateProfileId,
                    $"{x.JobApplication.CandidateProfile.FirstName} {x.JobApplication.CandidateProfile.LastName}"
                        .Trim(),
                    x.JobApplication.JobId,
                    x.JobApplication.Job.Title,
                    x.EvaluatorUserId,
                    users.GetValueOrDefault(
                        x.EvaluatorUserId,
                        "Unknown user"),
                    x.SkillsScore,
                    x.ExperienceScore,
                    x.InterviewScore,
                    x.OverallScore,
                    x.Comments,
                    x.UpdatedAtUtc ??
                        x.CreatedAtUtc))
            .ToList();
    }

    private static decimal
        CalculateOverall(
            decimal skills,
            decimal experience,
            decimal interview)
    {
        return Math.Round(
            skills * 0.30m +
            experience * 0.20m +
            interview * 0.50m,
            2);
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
