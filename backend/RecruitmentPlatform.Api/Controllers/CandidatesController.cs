using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Candidates;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.Storage;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class CandidatesController : ControllerBase
{
    private static readonly HashSet<string> AllowedResumeExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".docx"
        };

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJobMatchingService _matchingService;
    private readonly IFileStorageService _fileStorageService;

    public CandidatesController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IJobMatchingService matchingService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _matchingService = matchingService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> GetMyProfile()
    {
        var userId = GetCurrentUserId();

        var candidate = await CandidateProfileQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        var user = await _userManager.FindByIdAsync(userId);

        return Ok(ToProfileResponse(
            candidate,
            user?.Email ?? string.Empty));
    }

    [HttpPut("me")]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<CandidateProfileResponse>> UpdateMyProfile(
        UpdateCandidateProfileRequest request)
    {
        var userId = GetCurrentUserId();

        var candidate = await CandidateProfileQuery()
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Unauthorized();
        }

        candidate.FirstName = request.FirstName.Trim();
        candidate.LastName = request.LastName.Trim();
        candidate.Phone = Clean(request.Phone);
        candidate.Location = Clean(request.Location);
        candidate.CurrentJobTitle =
            Clean(request.CurrentJobTitle);
        candidate.ProfessionalSummary =
            Clean(request.ProfessionalSummary);
        candidate.YearsOfExperience =
            request.YearsOfExperience;
        candidate.LinkedInUrl = Clean(request.LinkedInUrl);
        candidate.PortfolioUrl = Clean(request.PortfolioUrl);

        user.FirstName = candidate.FirstName;
        user.LastName = candidate.LastName;

        var userUpdate = await _userManager.UpdateAsync(user);

        if (!userUpdate.Succeeded)
        {
            return BadRequest(new
            {
                message = "Could not update the user account.",
                errors = userUpdate.Errors.Select(
                    x => x.Description)
            });
        }

        await ReplaceCandidateSkills(
            candidate,
            request.Skills);

        candidate.IsProfileComplete =
            CalculateCompleteness(candidate) >= 80;

        AddAudit(
            "Update",
            "CandidateProfile",
            candidate.Id,
            $"{candidate.FirstName} {candidate.LastName}");

        await _dbContext.SaveChangesAsync();

        return Ok(ToProfileResponse(
            candidate,
            user.Email ?? string.Empty));
    }

    [HttpPost("me/resume")]
    [Authorize(Roles = "Candidate")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ResumeResponse>> UploadResume(
        IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Select a resume file."
            });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message = "Resume files must be 5 MB or smaller."
            });
        }

        var extension =
            Path.GetExtension(file.FileName);

        if (!AllowedResumeExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Only PDF and DOCX files are allowed."
            });
        }

        var userId = GetCurrentUserId();

        var candidate = await _dbContext.CandidateProfiles
            .Include(x => x.CandidateSkills)
            .Include(x => x.Resumes)
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        foreach (var existingResume in candidate.Resumes)
        {
            existingResume.IsPrimary = false;
        }

        var contentType =
            string.IsNullOrWhiteSpace(
                file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

        StoredFileResult storedFile;

        await using (var source =
            file.OpenReadStream())
        {
            storedFile =
                await _fileStorageService
                    .SaveAsync(
                        source,
                        file.FileName,
                        contentType,
                        $"resumes/{userId}");
        }

        var resume = new Resume
        {
            CandidateProfileId =
                candidate.Id,
            OriginalFileName =
                Path.GetFileName(
                    file.FileName),
            StoredFileName =
                storedFile.StoredFileName,
            StoragePath =
                storedFile.StorageKey,
            ContentType = contentType,
            FileSizeBytes =
                file.Length,
            IsPrimary = true,
            UploadedAtUtc =
                DateTime.UtcNow
        };

        _dbContext.Resumes.Add(
            resume);

        candidate.IsProfileComplete =
            CalculateCompleteness(
                candidate,
                hasResume: true) >= 80;

        AddAudit(
            "Upload",
            "Resume",
            resume.Id,
            $"{resume.OriginalFileName} ({storedFile.Provider})");

        try
        {
            await _dbContext
                .SaveChangesAsync();
        }
        catch
        {
            await _fileStorageService
                .DeleteAsync(
                    storedFile.StorageKey);

            throw;
        }

        return Ok(
            ToResumeResponse(resume));
    }

    [HttpGet("me/resume/{resumeId:guid}/download")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> DownloadResume(
        Guid resumeId)
    {
        var userId = GetCurrentUserId();

        var resume = await _dbContext.Resumes
            .AsNoTracking()
            .Include(x => x.CandidateProfile)
            .SingleOrDefaultAsync(x =>
                x.Id == resumeId &&
                x.CandidateProfile.UserId == userId);

        if (resume is null)
        {
            return NotFound(new
            {
                message =
                    "Resume file not found."
            });
        }

        var stream =
            await _fileStorageService
                .OpenReadAsync(
                    resume.StoragePath);

        if (stream is null)
        {
            return NotFound(new
            {
                message =
                    "Resume file not found."
            });
        }

        return File(
            stream,
            resume.ContentType,
            resume.OriginalFileName,
            enableRangeProcessing: true);
    }

    [HttpGet("me/recommendations")]
    [Authorize(Roles = "Candidate")]
    public async Task<
        ActionResult<IReadOnlyCollection<JobRecommendationResponse>>>
        GetRecommendations()
    {
        var userId = GetCurrentUserId();

        var candidate = await CandidateProfileQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate profile not found."
            });
        }

        var now = DateTime.UtcNow;

        var jobs = await _dbContext.Jobs
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .Include(x => x.JobSkills)
                .ThenInclude(x => x.Skill)
            .Where(x =>
                x.Status == JobStatus.Published &&
                (!x.ClosingAtUtc.HasValue ||
                 x.ClosingAtUtc.Value >= now))
            .ToListAsync();

        var recommendations = jobs
            .Select(job =>
            {
                var match =
                    _matchingService.Calculate(candidate, job);

                return new JobRecommendationResponse(
                    job.Id,
                    job.Title,
                    job.Organization.Name,
                    job.Department?.Name ?? "General",
                    job.Location,
                    FormatEmploymentType(
                        job.EmploymentType),
                    FormatWorkMode(job.WorkMode),
                    FormatSalary(
                        job.MinimumSalary,
                        job.MaximumSalary,
                        job.Currency),
                    match.Score,
                    match.Reason,
                    match.MatchedSkills,
                    match.MissingSkills);
            })
            .OrderByDescending(x => x.MatchScore)
            .ThenBy(x => x.Title)
            .Take(20)
            .ToList();

        return Ok(recommendations);
    }

    [HttpGet]
    [Authorize(Roles =
        "Recruiter,HiringManager,Administrator")]
    public async Task<
        ActionResult<IReadOnlyCollection<CandidateSummaryResponse>>>
        List(
            [FromQuery] string? search,
            [FromQuery] string? skill)
    {
        var query = CandidateProfileQuery()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(x =>
                x.FirstName.ToLower().Contains(term) ||
                x.LastName.ToLower().Contains(term) ||
                (x.CurrentJobTitle != null &&
                 x.CurrentJobTitle.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(skill))
        {
            var skillTerm = skill.Trim().ToLower();

            query = query.Where(x =>
                x.CandidateSkills.Any(candidateSkill =>
                    candidateSkill.Skill.Name.ToLower() ==
                    skillTerm));
        }

        var candidates = await query
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();

        var userIds = candidates
            .Select(x => x.UserId)
            .ToArray();

        var emails = await _dbContext.Users
            .AsNoTracking()
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Email ?? string.Empty);

        return Ok(candidates.Select(candidate =>
            new CandidateSummaryResponse(
                candidate.Id,
                candidate.UserId,
                $"{candidate.FirstName} {candidate.LastName}".Trim(),
                emails.GetValueOrDefault(
                    candidate.UserId,
                    string.Empty),
                candidate.Phone,
                candidate.Location,
                candidate.CurrentJobTitle,
                candidate.YearsOfExperience,
                candidate.CandidateSkills
                    .Select(x => x.Skill.Name)
                    .OrderBy(x => x)
                    .ToArray(),
                CalculateCompleteness(candidate)))
            .ToList());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles =
        "Recruiter,HiringManager,Administrator")]
    public async Task<ActionResult<CandidateProfileResponse>> Get(
        Guid id)
    {
        var candidate = await CandidateProfileQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (candidate is null)
        {
            return NotFound(new
            {
                message = "Candidate not found."
            });
        }

        var user =
            await _userManager.FindByIdAsync(
                candidate.UserId);

        return Ok(ToProfileResponse(
            candidate,
            user?.Email ?? string.Empty));
    }

    private IQueryable<CandidateProfile> CandidateProfileQuery()
    {
        return _dbContext.CandidateProfiles
            .Include(x => x.CandidateSkills)
                .ThenInclude(x => x.Skill)
            .Include(x => x.Resumes);
    }

    private async Task ReplaceCandidateSkills(
        CandidateProfile candidate,
        IEnumerable<string>? skillNames)
    {
        _dbContext.CandidateSkills.RemoveRange(
            candidate.CandidateSkills);

        candidate.CandidateSkills.Clear();

        var normalizedNames = (skillNames ?? [])
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToList();

        foreach (var skillName in normalizedNames)
        {
            var lowerName =
                skillName.ToLowerInvariant();

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

            candidate.CandidateSkills.Add(
                new CandidateSkill
                {
                    CandidateProfile = candidate,
                    Skill = skill,
                    ProficiencyLevel = 1
                });
        }
    }

    private CandidateProfileResponse ToProfileResponse(
        CandidateProfile candidate,
        string email)
    {
        var primaryResume = candidate.Resumes
            .Where(x => x.IsPrimary)
            .OrderByDescending(x => x.UploadedAtUtc)
            .FirstOrDefault();

        var completeness =
            CalculateCompleteness(candidate);

        return new CandidateProfileResponse(
            candidate.Id,
            candidate.UserId,
            candidate.FirstName,
            candidate.LastName,
            $"{candidate.FirstName} {candidate.LastName}".Trim(),
            email,
            candidate.Phone,
            candidate.Location,
            candidate.CurrentJobTitle,
            candidate.ProfessionalSummary,
            candidate.YearsOfExperience,
            candidate.LinkedInUrl,
            candidate.PortfolioUrl,
            completeness >= 80,
            completeness,
            candidate.CandidateSkills
                .Select(x => x.Skill.Name)
                .OrderBy(x => x)
                .ToArray(),
            primaryResume is null
                ? null
                : ToResumeResponse(primaryResume));
    }

    private static ResumeResponse ToResumeResponse(
        Resume resume)
    {
        return new ResumeResponse(
            resume.Id,
            resume.OriginalFileName,
            resume.ContentType,
            resume.FileSizeBytes,
            resume.UploadedAtUtc,
            resume.IsPrimary);
    }

    private static int CalculateCompleteness(
        CandidateProfile candidate,
        bool? hasResume = null)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(
            candidate.FirstName) &&
            !string.IsNullOrWhiteSpace(
                candidate.LastName))
        {
            score += 15;
        }

        if (!string.IsNullOrWhiteSpace(candidate.Phone))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(
            candidate.Location))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(
            candidate.CurrentJobTitle))
        {
            score += 15;
        }

        if (!string.IsNullOrWhiteSpace(
            candidate.ProfessionalSummary))
        {
            score += 15;
        }

        if (candidate.YearsOfExperience > 0)
        {
            score += 10;
        }

        if (candidate.CandidateSkills.Count > 0)
        {
            score += 15;
        }

        var resumeExists = hasResume ??
            candidate.Resumes.Any(x => x.IsPrimary);

        if (resumeExists)
        {
            score += 10;
        }

        return Math.Clamp(score, 0, 100);
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

    private static string FormatEmploymentType(
        EmploymentType employmentType)
    {
        return employmentType switch
        {
            EmploymentType.FullTime => "Full-time",
            EmploymentType.PartTime => "Part-time",
            _ => employmentType.ToString()
        };
    }

    private static string FormatWorkMode(
        WorkMode workMode)
    {
        return workMode switch
        {
            WorkMode.OnSite => "On-site",
            _ => workMode.ToString()
        };
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
}
