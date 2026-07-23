using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Jobs;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public JobsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<JobResponse>>> List(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? employmentType,
        [FromQuery] bool mine = false)
    {
        var query = _dbContext.Jobs
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .Include(x => x.RecruiterProfile)
            .Include(x => x.JobSkills)
                .ThenInclude(x => x.Skill)
            .AsQueryable();

        if (mine)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Administrator"))
            {
                var userId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                var recruiterId = await _dbContext.RecruiterProfiles
                    .Where(x => x.UserId == userId)
                    .Select(x => (Guid?)x.Id)
                    .SingleOrDefaultAsync();

                if (!recruiterId.HasValue)
                {
                    return Ok(Array.Empty<JobResponse>());
                }

                query = query.Where(
                    x => x.RecruiterProfileId == recruiterId.Value);
            }
        }
        else
        {
            var now = DateTime.UtcNow;

            query = query.Where(x =>
                x.Status == JobStatus.Published &&
                (!x.ClosingAtUtc.HasValue ||
                 x.ClosingAtUtc.Value >= now));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(x =>
                x.Title.ToLower().Contains(term) ||
                x.Description.ToLower().Contains(term) ||
                x.Location.ToLower().Contains(term) ||
                x.JobSkills.Any(js =>
                    js.Skill.Name.ToLower().Contains(term)));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(
                x => x.DepartmentId == departmentId.Value);
        }

        if (TryParseEmploymentType(
            employmentType,
            out var parsedEmploymentType))
        {
            query = query.Where(
                x => x.EmploymentType == parsedEmploymentType);
        }

        var jobs = await query
            .OrderByDescending(x =>
                x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ToListAsync();

        return Ok(jobs.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobResponse>> Get(Guid id)
    {
        var job = await JobQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return NotFound(new { message = "Job not found." });
        }

        if (job.Status != JobStatus.Published &&
            !await CanManageJob(job))
        {
            return NotFound(new { message = "Job not found." });
        }

        return Ok(ToResponse(job));
    }

    [HttpPost]
    [Authorize(Roles = "Recruiter,Administrator")]
    public async Task<ActionResult<JobResponse>> Create(
        SaveJobRequest request)
    {
        var ownership = await ResolveOwnership(request.OrganizationId);

        if (ownership.ErrorResult is not null)
        {
            return ownership.ErrorResult;
        }

        if (!await DepartmentBelongsToOrganization(
            request.DepartmentId,
            ownership.OrganizationId))
        {
            return BadRequest(new
            {
                message =
                    "The selected department does not belong to the organization."
            });
        }

        if (!TryParseEmploymentType(
            request.EmploymentType,
            out var employmentType))
        {
            return BadRequest(new
            {
                message = "Invalid employment type."
            });
        }

        if (!TryParseWorkMode(
            request.WorkMode,
            out var workMode))
        {
            return BadRequest(new
            {
                message = "Invalid work mode."
            });
        }

        if (!TryParseJobStatus(
            request.Status,
            out var status))
        {
            return BadRequest(new
            {
                message = "Invalid job status."
            });
        }

        var salaryError = ValidateSalary(request);
        if (salaryError is not null)
        {
            return BadRequest(new { message = salaryError });
        }

        var job = new Job
        {
            OrganizationId = ownership.OrganizationId,
            DepartmentId = request.DepartmentId,
            RecruiterProfileId = ownership.RecruiterProfileId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Responsibilities = request.Responsibilities?.Trim(),
            Requirements = request.Requirements?.Trim(),
            Location = request.Location.Trim(),
            EmploymentType = employmentType,
            WorkMode = workMode,
            MinimumExperienceYears =
                request.MinimumExperienceYears,
            MinimumSalary = request.MinimumSalary,
            MaximumSalary = request.MaximumSalary,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Status = status,
            PublishedAtUtc =
                status == JobStatus.Published
                    ? DateTime.UtcNow
                    : null,
            ClosingAtUtc = request.ClosingAtUtc
        };

        await SyncSkills(job, request.Skills);

        _dbContext.Jobs.Add(job);
        AddAudit("Create", "Job", job.Id, job.Title);
        await _dbContext.SaveChangesAsync();

        var created = await JobQuery()
            .AsNoTracking()
            .SingleAsync(x => x.Id == job.Id);

        return CreatedAtAction(
            nameof(Get),
            new { id = job.Id },
            ToResponse(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Recruiter,Administrator")]
    public async Task<ActionResult<JobResponse>> Update(
        Guid id,
        SaveJobRequest request)
    {
        var job = await _dbContext.Jobs
            .Include(x => x.JobSkills)
                .ThenInclude(x => x.Skill)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return NotFound(new { message = "Job not found." });
        }

        if (!await CanManageJob(job))
        {
            return Forbid();
        }

        var ownership = await ResolveOwnership(request.OrganizationId);

        if (ownership.ErrorResult is not null)
        {
            return ownership.ErrorResult;
        }

        if (!await DepartmentBelongsToOrganization(
            request.DepartmentId,
            ownership.OrganizationId))
        {
            return BadRequest(new
            {
                message =
                    "The selected department does not belong to the organization."
            });
        }

        if (!TryParseEmploymentType(
            request.EmploymentType,
            out var employmentType) ||
            !TryParseWorkMode(
                request.WorkMode,
                out var workMode) ||
            !TryParseJobStatus(
                request.Status,
                out var status))
        {
            return BadRequest(new
            {
                message =
                    "Employment type, work mode, or status is invalid."
            });
        }

        var salaryError = ValidateSalary(request);
        if (salaryError is not null)
        {
            return BadRequest(new { message = salaryError });
        }

        job.OrganizationId = ownership.OrganizationId;
        job.DepartmentId = request.DepartmentId;
        job.RecruiterProfileId =
            ownership.RecruiterProfileId ??
            job.RecruiterProfileId;
        job.Title = request.Title.Trim();
        job.Description = request.Description.Trim();
        job.Responsibilities = request.Responsibilities?.Trim();
        job.Requirements = request.Requirements?.Trim();
        job.Location = request.Location.Trim();
        job.EmploymentType = employmentType;
        job.WorkMode = workMode;
        job.MinimumExperienceYears =
            request.MinimumExperienceYears;
        job.MinimumSalary = request.MinimumSalary;
        job.MaximumSalary = request.MaximumSalary;
        job.Currency = request.Currency.Trim().ToUpperInvariant();
        job.ClosingAtUtc = request.ClosingAtUtc;

        if (job.Status != JobStatus.Published &&
            status == JobStatus.Published)
        {
            job.PublishedAtUtc = DateTime.UtcNow;
        }

        job.Status = status;

        await SyncSkills(job, request.Skills);

        AddAudit("Update", "Job", job.Id, job.Title);
        await _dbContext.SaveChangesAsync();

        var updated = await JobQuery()
            .AsNoTracking()
            .SingleAsync(x => x.Id == job.Id);

        return Ok(ToResponse(updated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Recruiter,Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var job = await _dbContext.Jobs
            .Include(x => x.Applications)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (job is null)
        {
            return NotFound(new { message = "Job not found." });
        }

        if (!await CanManageJob(job))
        {
            return Forbid();
        }

        if (job.Applications.Count > 0)
        {
            return Conflict(new
            {
                message =
                    "A job with applications cannot be deleted. Close or archive it instead."
            });
        }

        _dbContext.Jobs.Remove(job);
        AddAudit("Delete", "Job", job.Id, job.Title);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Job> JobQuery()
    {
        return _dbContext.Jobs
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .Include(x => x.RecruiterProfile)
            .Include(x => x.JobSkills)
                .ThenInclude(x => x.Skill);
    }

    private async Task<bool> CanManageJob(Job job)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (User.IsInRole("Administrator"))
        {
            return true;
        }

        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var recruiterId = await _dbContext.RecruiterProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync();

        return recruiterId.HasValue &&
            job.RecruiterProfileId == recruiterId.Value;
    }

    private async Task<OwnershipResult> ResolveOwnership(
        Guid? requestedOrganizationId)
    {
        if (User.IsInRole("Administrator"))
        {
            if (!requestedOrganizationId.HasValue)
            {
                return new OwnershipResult(
                    Guid.Empty,
                    null,
                    BadRequest(new
                    {
                        message =
                            "Organization is required for administrator-created jobs."
                    }));
            }

            var exists = await _dbContext.Organizations
                .AnyAsync(x =>
                    x.Id == requestedOrganizationId.Value &&
                    x.IsActive);

            return exists
                ? new OwnershipResult(
                    requestedOrganizationId.Value,
                    null,
                    null)
                : new OwnershipResult(
                    Guid.Empty,
                    null,
                    BadRequest(new
                    {
                        message =
                            "The selected organization does not exist or is inactive."
                    }));
        }

        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        var recruiter = await _dbContext.RecruiterProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (recruiter?.OrganizationId is null)
        {
            return new OwnershipResult(
                Guid.Empty,
                null,
                BadRequest(new
                {
                    message =
                        "Your recruiter account is not assigned to an organization."
                }));
        }

        return new OwnershipResult(
            recruiter.OrganizationId.Value,
            recruiter.Id,
            null);
    }

    private async Task<bool> DepartmentBelongsToOrganization(
        Guid? departmentId,
        Guid organizationId)
    {
        if (!departmentId.HasValue)
        {
            return true;
        }

        return await _dbContext.Departments.AnyAsync(x =>
            x.Id == departmentId.Value &&
            x.OrganizationId == organizationId &&
            x.IsActive);
    }

    private async Task SyncSkills(
        Job job,
        IEnumerable<string>? skillNames)
    {
        var normalizedNames = (skillNames ?? [])
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(30)
            .ToList();

        var desiredNames = normalizedNames
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var removedLinks = job.JobSkills
            .Where(link =>
                link.Skill is not null &&
                !desiredNames.Contains(link.Skill.Name))
            .ToList();

        if (removedLinks.Count > 0)
        {
            _dbContext.JobSkills.RemoveRange(removedLinks);

            foreach (var link in removedLinks)
            {
                job.JobSkills.Remove(link);
            }
        }

        foreach (var skillName in normalizedNames)
        {
            var alreadyLinked = job.JobSkills.Any(link =>
                link.Skill is not null &&
                string.Equals(
                    link.Skill.Name,
                    skillName,
                    StringComparison.OrdinalIgnoreCase));

            if (alreadyLinked)
            {
                continue;
            }

            var lowerName = skillName.ToLowerInvariant();

            var skill = await _dbContext.Skills
                .SingleOrDefaultAsync(x =>
                    x.Name.ToLower() == lowerName);

            if (skill is null)
            {
                skill = new Skill
                {
                    Name = skillName
                };

                _dbContext.Skills.Add(skill);
            }

            job.JobSkills.Add(new JobSkill
            {
                Job = job,
                Skill = skill,
                IsRequired = true,
                Weight = 1m
            });
        }
    }

    private static string? ValidateSalary(SaveJobRequest request)
    {
        if (request.MinimumSalary.HasValue &&
            request.MaximumSalary.HasValue &&
            request.MinimumSalary.Value >
            request.MaximumSalary.Value)
        {
            return
                "Minimum salary cannot be greater than maximum salary.";
        }

        return null;
    }

    private static bool TryParseEmploymentType(
        string? value,
        out EmploymentType employmentType)
    {
        var normalized = value?
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

        return Enum.TryParse(
            normalized,
            true,
            out employmentType);
    }

    private static bool TryParseWorkMode(
        string? value,
        out WorkMode workMode)
    {
        var normalized = value?
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty);

        return Enum.TryParse(
            normalized,
            true,
            out workMode);
    }

    private static bool TryParseJobStatus(
        string? value,
        out JobStatus status)
    {
        if (string.Equals(
            value,
            "Open",
            StringComparison.OrdinalIgnoreCase))
        {
            status = JobStatus.Published;
            return true;
        }

        return Enum.TryParse(value, true, out status);
    }

    private static JobResponse ToResponse(Job job)
    {
        var status = job.Status == JobStatus.Published
            ? "Open"
            : job.Status.ToString();

        var type = job.EmploymentType switch
        {
            EmploymentType.FullTime => "Full-time",
            EmploymentType.PartTime => "Part-time",
            _ => job.EmploymentType.ToString()
        };

        var remote = job.WorkMode switch
        {
            WorkMode.OnSite => "On-site",
            _ => job.WorkMode.ToString()
        };

        var recruiter =
            job.RecruiterProfile is null
                ? "Administrator"
                : $"{job.RecruiterProfile.FirstName} {job.RecruiterProfile.LastName}".Trim();

        return new JobResponse(
            job.Id,
            job.OrganizationId,
            job.Organization.Name,
            job.DepartmentId,
            job.Department?.Name ?? "General",
            recruiter,
            job.Title,
            job.Description,
            job.Responsibilities,
            job.Requirements,
            job.Location,
            type,
            remote,
            job.MinimumExperienceYears,
            job.MinimumExperienceYears == 0
                ? "Entry level"
                : $"{job.MinimumExperienceYears}+ years",
            job.MinimumSalary,
            job.MaximumSalary,
            job.Currency,
            FormatSalary(
                job.MinimumSalary,
                job.MaximumSalary,
                job.Currency),
            status,
            job.PublishedAtUtc,
            job.ClosingAtUtc,
            FormatPosted(
                job.PublishedAtUtc ?? job.CreatedAtUtc),
            job.JobSkills
                .Select(x => x.Skill.Name)
                .OrderBy(x => x)
                .ToArray());
    }

    private static string FormatSalary(
        decimal? minimum,
        decimal? maximum,
        string currency)
    {
        if (!minimum.HasValue && !maximum.HasValue)
        {
            return "Not disclosed";
        }

        if (minimum.HasValue && maximum.HasValue)
        {
            return
                $"{currency} {minimum.Value:N0} - {maximum.Value:N0}";
        }

        return minimum.HasValue
            ? $"From {currency} {minimum.Value:N0}"
            : $"Up to {currency} {maximum!.Value:N0}";
    }

    private static string FormatPosted(DateTime postedAtUtc)
    {
        var days = Math.Max(
            0,
            (DateTime.UtcNow.Date - postedAtUtc.Date).Days);

        return days switch
        {
            0 => "today",
            1 => "1 day ago",
            _ => $"{days} days ago"
        };
    }

    private void AddAudit(
        string action,
        string entityName,
        Guid entityId,
        string details)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId =
                User.FindFirstValue(ClaimTypes.NameIdentifier),
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            Details = details,
            IpAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }

    private sealed record OwnershipResult(
        Guid OrganizationId,
        Guid? RecruiterProfileId,
        ActionResult? ErrorResult);
}
