using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Applications;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IJobMatchingService _matchingService;

    public ApplicationsController(
        ApplicationDbContext dbContext,
        IJobMatchingService matchingService)
    {
        _dbContext = dbContext;
        _matchingService = matchingService;
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<ApplicationResponse>>>
        List(
            [FromQuery] Guid? jobId,
            [FromQuery] string? status)
    {
        var query = ApplicationQuery()
            .AsNoTracking()
            .AsQueryable();

        var userId = GetCurrentUserId();

        if (User.IsInRole("Candidate"))
        {
            query = query.Where(x =>
                x.CandidateProfile.UserId == userId);
        }
        else if (User.IsInRole("Recruiter"))
        {
            query = query.Where(x =>
                x.Job.RecruiterProfile != null &&
                x.Job.RecruiterProfile.UserId == userId);
        }
        else if (User.IsInRole("HiringManager"))
        {
            var manager = await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId == userId);

            if (manager?.OrganizationId is null)
            {
                return Ok(
                    Array.Empty<ApplicationResponse>());
            }

            query = query.Where(x =>
                x.Job.OrganizationId ==
                    manager.OrganizationId.Value &&
                (!manager.DepartmentId.HasValue ||
                 x.Job.DepartmentId ==
                    manager.DepartmentId) &&
                (x.Status ==
                    ApplicationStatus.UnderReview ||
                 x.Status ==
                    ApplicationStatus.Shortlisted ||
                 x.Status ==
                    ApplicationStatus.InterviewScheduled ||
                 x.Status ==
                    ApplicationStatus.Offered ||
                 x.Status ==
                    ApplicationStatus.Hired ||
                 x.Status ==
                    ApplicationStatus.Rejected));
        }
        else if (!User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        if (jobId.HasValue)
        {
            query = query.Where(
                x => x.JobId == jobId.Value);
        }

        if (TryParseStatus(status, out var parsedStatus))
        {
            query = query.Where(
                x => x.Status == parsedStatus);
        }

        var applications = await query
            .OrderByDescending(x => x.AppliedAtUtc)
            .ToListAsync();

        return Ok(applications
            .Select(ToResponse)
            .ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationResponse>> Get(
        Guid id)
    {
        var application = await ApplicationQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (application is null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        if (!await CanView(application))
        {
            return Forbid();
        }

        return Ok(ToResponse(application));
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<ApplicationResponse>> Create(
        CreateApplicationRequest request)
    {
        var userId = GetCurrentUserId();

        var candidate = await _dbContext.CandidateProfiles
            .Include(x => x.CandidateSkills)
                .ThenInclude(x => x.Skill)
            .Include(x => x.Resumes)
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        var job = await _dbContext.Jobs
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .Include(x => x.RecruiterProfile)
            .Include(x => x.JobSkills)
                .ThenInclude(x => x.Skill)
            .SingleOrDefaultAsync(x =>
                x.Id == request.JobId);

        if (job is null)
        {
            return NotFound(new
            {
                message = "Job not found."
            });
        }

        if (job.Status != JobStatus.Published)
        {
            return BadRequest(new
            {
                message =
                    "Applications are accepted only for published jobs."
            });
        }

        if (job.ClosingAtUtc.HasValue &&
            job.ClosingAtUtc.Value < DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "The application closing date has passed."
            });
        }

        var duplicate = await _dbContext.JobApplications
            .AnyAsync(x =>
                x.CandidateProfileId == candidate.Id &&
                x.JobId == job.Id);

        if (duplicate)
        {
            return Conflict(new
            {
                message =
                    "You have already applied for this job."
            });
        }

        var match =
            _matchingService.Calculate(candidate, job);

        var application = new JobApplication
        {
            CandidateProfileId = candidate.Id,
            JobId = job.Id,
            CoverLetter = Clean(request.CoverLetter),
            Status = ApplicationStatus.Submitted,
            Stage = "Screening",
            MatchScore = match.Score,
            AppliedAtUtc = DateTime.UtcNow
        };

        _dbContext.JobApplications.Add(application);

        _dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = NotificationType.ApplicationUpdate,
            Title = "Application submitted",
            Message =
                $"Your application for {job.Title} was submitted."
        });

        AddAudit(
            "Create",
            "JobApplication",
            application.Id,
            $"{candidate.FirstName} {candidate.LastName} -> {job.Title}");

        await _dbContext.SaveChangesAsync();

        var created = await ApplicationQuery()
            .AsNoTracking()
            .SingleAsync(x => x.Id == application.Id);

        return CreatedAtAction(
            nameof(Get),
            new { id = application.Id },
            ToResponse(created));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Recruiter,Administrator")]
    public async Task<ActionResult<ApplicationResponse>>
        UpdateStatus(
            Guid id,
            UpdateApplicationStatusRequest request)
    {
        var application = await ApplicationQuery()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (application is null)
        {
            return NotFound(new
            {
                message = "Application not found."
            });
        }

        if (!CanManage(application))
        {
            return Forbid();
        }

        if (!TryParseStatus(
            request.Status,
            out var newStatus))
        {
            return BadRequest(new
            {
                message = "Invalid application status."
            });
        }

        application.Status = newStatus;
        application.Stage =
            string.IsNullOrWhiteSpace(request.Stage)
                ? DefaultStage(newStatus)
                : request.Stage.Trim();

        _dbContext.Notifications.Add(new Notification
        {
            UserId = application.CandidateProfile.UserId,
            Type = NotificationType.ApplicationUpdate,
            Title = "Application status updated",
            Message =
                $"Your application for {application.Job.Title} is now {DisplayStatus(newStatus)}."
        });

        AddAudit(
            "UpdateStatus",
            "JobApplication",
            application.Id,
            $"{application.Job.Title}: {DisplayStatus(newStatus)}");

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(application));
    }

    private IQueryable<JobApplication> ApplicationQuery()
    {
        return _dbContext.JobApplications
            .Include(x => x.CandidateProfile)
                .ThenInclude(x => x.CandidateSkills)
                    .ThenInclude(x => x.Skill)
            .Include(x => x.Job)
                .ThenInclude(x => x.Organization)
            .Include(x => x.Job)
                .ThenInclude(x => x.Department)
            .Include(x => x.Job)
                .ThenInclude(x => x.RecruiterProfile)
            .Include(x => x.Job)
                .ThenInclude(x => x.JobSkills)
                    .ThenInclude(x => x.Skill);
    }

    private ApplicationResponse ToResponse(
        JobApplication application)
    {
        var match = _matchingService.Calculate(
            application.CandidateProfile,
            application.Job);

        return new ApplicationResponse(
            application.Id,
            application.CandidateProfileId,
            $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName}".Trim(),
            UserEmail(application.CandidateProfile.UserId),
            application.CandidateProfile.Phone,
            application.CandidateProfile.CurrentJobTitle,
            application.CandidateProfile.Location,
            application.CandidateProfile.YearsOfExperience,
            application.CandidateProfile.CandidateSkills
                .Select(x => x.Skill.Name)
                .OrderBy(x => x)
                .ToArray(),
            application.JobId,
            application.Job.Title,
            application.Job.Organization.Name,
            application.Job.Department?.Name ?? "General",
            application.AppliedAtUtc,
            application.AppliedAtUtc.ToString("yyyy-MM-dd"),
            DisplayStatus(application.Status),
            application.Stage,
            application.MatchScore,
            match.MatchedSkills,
            match.MissingSkills,
            match.Reason,
            application.CoverLetter);
    }

    private string UserEmail(string userId)
    {
        return _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(x => x.Email ?? string.Empty)
            .FirstOrDefault() ?? string.Empty;
    }

    private async Task<bool> CanView(
        JobApplication application)
    {
        if (User.IsInRole("Administrator"))
        {
            return true;
        }

        var userId = GetCurrentUserId();

        if (User.IsInRole("Candidate"))
        {
            return application.CandidateProfile.UserId ==
                userId;
        }

        if (User.IsInRole("Recruiter"))
        {
            return application.Job.RecruiterProfile?.UserId ==
                userId;
        }

        if (User.IsInRole("HiringManager"))
        {
            var manager = await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId == userId);

            if (manager?.OrganizationId is null)
            {
                return false;
            }

            var statusAllowed =
                application.Status ==
                    ApplicationStatus.UnderReview ||
                application.Status ==
                    ApplicationStatus.Shortlisted ||
                application.Status ==
                    ApplicationStatus.InterviewScheduled ||
                application.Status ==
                    ApplicationStatus.Offered ||
                application.Status ==
                    ApplicationStatus.Hired ||
                application.Status ==
                    ApplicationStatus.Rejected;

            return statusAllowed &&
                application.Job.OrganizationId ==
                    manager.OrganizationId.Value &&
                (!manager.DepartmentId.HasValue ||
                 application.Job.DepartmentId ==
                    manager.DepartmentId);
        }

        return false;
    }

    private bool CanManage(
        JobApplication application)
    {
        if (User.IsInRole("Administrator"))
        {
            return true;
        }

        return User.IsInRole("Recruiter") &&
            application.Job.RecruiterProfile?.UserId ==
            GetCurrentUserId();
    }

    private static bool TryParseStatus(
        string? value,
        out ApplicationStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = default;
            return false;
        }

        if (string.Equals(
            value,
            "Pending",
            StringComparison.OrdinalIgnoreCase))
        {
            status = ApplicationStatus.UnderReview;
            return true;
        }

        return Enum.TryParse(
            value.Replace(" ", string.Empty),
            true,
            out status);
    }

    private static string DisplayStatus(
        ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Submitted => "Pending",
            ApplicationStatus.UnderReview => "Pending",
            ApplicationStatus.InterviewScheduled =>
                "Interview Scheduled",
            _ => status.ToString()
        };
    }

    private static string DefaultStage(
        ApplicationStatus status)
    {
        return status switch
        {
            ApplicationStatus.Submitted => "Screening",
            ApplicationStatus.UnderReview => "Screening",
            ApplicationStatus.Shortlisted => "Shortlist",
            ApplicationStatus.InterviewScheduled =>
                "Interview",
            ApplicationStatus.Offered => "Offer",
            ApplicationStatus.Hired => "Hired",
            ApplicationStatus.Rejected => "Closed",
            ApplicationStatus.Withdrawn => "Withdrawn",
            _ => "Screening"
        };
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
    }

    private void AddAudit(
        string action,
        string entityName,
        Guid entityId,
        string details)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = GetCurrentUserId(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            Details = details,
            IpAddress =
                HttpContext.Connection.RemoteIpAddress?
                    .ToString()
        });
    }
}
