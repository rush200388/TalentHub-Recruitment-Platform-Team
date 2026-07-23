using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.ResumeAnalysis;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.ResumeAnalysis;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/Candidates/me/resume")]
[Authorize(Roles = "Candidate")]
public sealed class ResumeAnalysisController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly IResumeTextExtractionService _textExtractionService;
    private readonly IResumeAnalysisService _analysisService;

    public ResumeAnalysisController(
        ApplicationDbContext dbContext,
        IResumeTextExtractionService textExtractionService,
        IResumeAnalysisService analysisService)
    {
        _dbContext = dbContext;
        _textExtractionService = textExtractionService;
        _analysisService = analysisService;
    }

    [HttpGet("analysis")]
    public async Task<ActionResult<ResumeAnalysisResponse>> GetAnalysis()
    {
        var resume = await GetPrimaryResumeQuery()
            .AsNoTracking()
            .SingleOrDefaultAsync();

        if (resume is null)
        {
            return NotFound(new
            {
                message =
                    "Upload a primary PDF or DOCX resume before running analysis."
            });
        }

        return Ok(ToResponse(resume));
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<ResumeAnalysisResponse>> Analyze(
        CancellationToken cancellationToken)
    {
        var resume = await GetPrimaryResumeQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (resume is null)
        {
            return NotFound(new
            {
                message =
                    "Upload a primary PDF or DOCX resume before running analysis."
            });
        }

        resume.AnalysisStatus = "Processing";
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var parsedText =
                await _textExtractionService.ExtractTextAsync(
                    resume.StoragePath,
                    resume.OriginalFileName,
                    cancellationToken);

            var result = _analysisService.Analyze(parsedText);

            resume.ParsedText = parsedText;
            resume.AnalysisStatus = "Completed";
            resume.AnalysisStrategy = _analysisService.StrategyName;
            resume.AnalysisJson = JsonSerializer.Serialize(
                result,
                JsonOptions);
            resume.AnalyzedAtUtc = DateTime.UtcNow;

            _dbContext.Notifications.Add(new Notification
            {
                UserId = GetCurrentUserId(),
                Type = NotificationType.System,
                Title = "Resume analysis completed",
                Message =
                    $"{result.ExtractedSkills.Count} skills were detected in {resume.OriginalFileName}."
            });

            AddAudit(
                "Analyze",
                "Resume",
                resume.Id,
                $"{resume.OriginalFileName}: {result.ExtractedSkills.Count} skills detected");

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ToResponse(resume));
        }
        catch (Exception exception) when (
            exception is InvalidOperationException or
            NotSupportedException or
            FileNotFoundException)
        {
            resume.AnalysisStatus = "Failed";
            resume.AnalysisJson = JsonSerializer.Serialize(
                new ResumeAnalysisResult(
                    0,
                    null,
                    null,
                    null,
                    [],
                    [],
                    [],
                    [exception.Message]),
                JsonOptions);
            resume.AnalyzedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("apply-skills")]
    public async Task<ActionResult<ApplyResumeSkillsResponse>> ApplySkills(
        ApplyResumeSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var resume = await GetPrimaryResumeQuery()
            .Include(x => x.CandidateProfile)
                .ThenInclude(x => x.CandidateSkills)
                    .ThenInclude(x => x.Skill)
            .SingleOrDefaultAsync(cancellationToken);

        if (resume is null)
        {
            return NotFound(new
            {
                message = "Primary resume not found."
            });
        }

        var analysis = DeserializeAnalysis(resume);

        if (analysis is null)
        {
            return BadRequest(new
            {
                message =
                    "Analyze the resume before applying extracted skills."
            });
        }

        var extracted = analysis.ExtractedSkills
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requested = request.Skills.Count == 0
            ? analysis.ExtractedSkills
            : request.Skills
                .Select(skill => skill.Trim())
                .Where(skill => !string.IsNullOrWhiteSpace(skill))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

        var invalid = requested
            .Where(skill => !extracted.Contains(skill))
            .ToArray();

        if (invalid.Length > 0)
        {
            return BadRequest(new
            {
                message =
                    "Only skills detected by the latest resume analysis can be applied.",
                invalidSkills = invalid
            });
        }

        var existingNames = resume.CandidateProfile.CandidateSkills
            .Select(x => x.Skill.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var added = new List<string>();

        foreach (var skillName in requested.Take(30))
        {
            if (existingNames.Contains(skillName))
            {
                continue;
            }

            var lowerName = skillName.ToLowerInvariant();

            var skill = await _dbContext.Skills
                .SingleOrDefaultAsync(
                    x => x.Name.ToLower() == lowerName,
                    cancellationToken);

            if (skill is null)
            {
                skill = new Skill
                {
                    Name = skillName
                };

                _dbContext.Skills.Add(skill);
            }

            resume.CandidateProfile.CandidateSkills.Add(
                new CandidateSkill
                {
                    CandidateProfile = resume.CandidateProfile,
                    Skill = skill,
                    ProficiencyLevel = 1
                });

            existingNames.Add(skillName);
            added.Add(skillName);
        }

        AddAudit(
            "ApplyExtractedSkills",
            "CandidateProfile",
            resume.CandidateProfileId,
            string.Join(", ", added));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var currentSkills = resume.CandidateProfile.CandidateSkills
            .Select(x => x.Skill.Name)
            .Concat(added)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        return Ok(new ApplyResumeSkillsResponse(
            added.Count,
            added,
            currentSkills));
    }

    private IQueryable<Resume> GetPrimaryResumeQuery()
    {
        var userId = GetCurrentUserId();

        return _dbContext.Resumes
            .Where(x =>
                x.IsPrimary &&
                x.CandidateProfile.UserId == userId)
            .OrderByDescending(x => x.UploadedAtUtc)
            .Take(1);
    }

    private static ResumeAnalysisResponse ToResponse(Resume resume)
    {
        var analysis = DeserializeAnalysis(resume);

        var preview = string.IsNullOrWhiteSpace(resume.ParsedText)
            ? null
            : resume.ParsedText.Length <= 1200
                ? resume.ParsedText
                : $"{resume.ParsedText[..1200]}…";

        return new ResumeAnalysisResponse(
            resume.Id,
            resume.OriginalFileName,
            resume.AnalysisStatus,
            resume.AnalysisStrategy,
            resume.AnalyzedAtUtc,
            analysis?.WordCount ?? 0,
            analysis?.ExtractedEmail,
            analysis?.ExtractedPhone,
            analysis?.SuggestedYearsOfExperience,
            analysis?.ExtractedSkills ?? [],
            analysis?.EducationSignals ?? [],
            analysis?.ExperienceSignals ?? [],
            analysis?.Warnings ?? [],
            preview);
    }

    private static ResumeAnalysisResult? DeserializeAnalysis(Resume resume)
    {
        if (string.IsNullOrWhiteSpace(resume.AnalysisJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ResumeAnalysisResult>(
                resume.AnalysisJson,
                JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
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
                HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }
}
