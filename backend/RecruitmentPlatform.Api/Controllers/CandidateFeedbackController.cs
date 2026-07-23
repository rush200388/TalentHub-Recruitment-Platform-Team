using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.AI;
using RecruitmentPlatform.Application.DTOs.AiFeedback;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route(
    "api/Applications/{applicationId:guid}/ai-feedback")]
[Authorize(
    Roles =
        "Recruiter,HiringManager,Administrator")]
public sealed class CandidateFeedbackController
    : ControllerBase
{
    private readonly ApplicationDbContext
        _dbContext;

    private readonly IJobMatchingService
        _matchingService;

    private readonly ICandidateFeedbackService
        _feedbackService;

    public CandidateFeedbackController(
        ApplicationDbContext dbContext,
        IJobMatchingService matchingService,
        ICandidateFeedbackService feedbackService)
    {
        _dbContext = dbContext;
        _matchingService =
            matchingService;
        _feedbackService =
            feedbackService;
    }

    [HttpPost]
    public async Task<
        ActionResult<
            CandidateFeedbackResponse>>
        Generate(
            Guid applicationId,
            CancellationToken
                cancellationToken)
    {
        var application =
            await ApplicationQuery()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        applicationId,
                    cancellationToken);

        if (application is null)
        {
            return NotFound(new
            {
                message =
                    "Application not found."
            });
        }

        if (!await CanReview(
            application,
            cancellationToken))
        {
            return Forbid();
        }

        var match =
            _matchingService.Calculate(
                application
                    .CandidateProfile,
                application.Job);

        var latestEvaluation =
            application.Evaluations
                .OrderByDescending(
                    evaluation =>
                        evaluation
                            .UpdatedAtUtc ??
                        evaluation
                            .CreatedAtUtc)
                .FirstOrDefault();

        var interviewFeedback =
            application.Interviews
                .SelectMany(
                    interview =>
                        interview.Feedback)
                .ToList();

        decimal? interviewScore =
    interviewFeedback.Count > 0
        ? Math.Round(
            interviewFeedback.Sum(
                item =>
                    (decimal)item.OverallRating)
            / interviewFeedback.Count
            * 20m,
            2)
        : null;

        var interviewRecommendation =
            interviewFeedback
                .OrderByDescending(
                    feedback =>
                        feedback
                            .UpdatedAtUtc ??
                        feedback
                            .CreatedAtUtc)
                .Select(
                    feedback =>
                        feedback
                            .Recommendation)
                .FirstOrDefault(
                    value =>
                        !string
                            .IsNullOrWhiteSpace(
                                value));

        var context =
            new CandidateFeedbackContext(
                application.Job.Title,
                application.Job
                    .MinimumExperienceYears,
                application
                    .CandidateProfile
                    .YearsOfExperience,
                application.MatchScore,
                match.MatchedSkills,
                match.MissingSkills,
                latestEvaluation?
                    .OverallScore,
                interviewScore,
                interviewRecommendation);

        var result =
            await _feedbackService
                .GenerateAsync(
                    context,
                    cancellationToken);

        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId =
                    GetCurrentUserId(),
                Action =
                    "GenerateAIFeedback",
                EntityName =
                    "JobApplication",
                EntityId =
                    application.Id
                        .ToString(),
                Details =
                    $"{result.Provider}: {result.Recommendation}",
                IpAddress =
                    HttpContext
                        .Connection
                        .RemoteIpAddress?
                        .ToString()
            });

        await _dbContext
            .SaveChangesAsync(
                cancellationToken);

        return Ok(
            new CandidateFeedbackResponse(
                application.Id,
                $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName}".Trim(),
                application.Job.Title,
                result.Provider,
                result.UsedExternalAi,
                result.Summary,
                result.Strengths,
                result.Risks,
                result.Recommendation,
                result.SuggestedFeedback,
                result.GeneratedAtUtc,
                result.FallbackReason));
    }

    private IQueryable<JobApplication>
        ApplicationQuery()
    {
        return _dbContext
            .JobApplications
            .Include(
                application =>
                    application
                        .CandidateProfile)
                .ThenInclude(
                    candidate =>
                        candidate
                            .CandidateSkills)
                    .ThenInclude(
                        candidateSkill =>
                            candidateSkill.Skill)
            .Include(
                application =>
                    application.Job)
                .ThenInclude(
                    job =>
                        job
                            .RecruiterProfile)
            .Include(
                application =>
                    application.Job)
                .ThenInclude(
                    job =>
                        job.JobSkills)
                    .ThenInclude(
                        jobSkill =>
                            jobSkill.Skill)
            .Include(
                application =>
                    application
                        .Evaluations)
            .Include(
                application =>
                    application.Interviews)
                .ThenInclude(
                    interview =>
                        interview.Feedback);
    }

    private async Task<bool> CanReview(
        JobApplication application,
        CancellationToken
            cancellationToken)
    {
        if (User.IsInRole(
            "Administrator"))
        {
            return true;
        }

        var userId =
            GetCurrentUserId();

        if (User.IsInRole(
            "Recruiter"))
        {
            return application.Job
                .RecruiterProfile?
                .UserId == userId;
        }

        if (!User.IsInRole(
            "HiringManager"))
        {
            return false;
        }

        var manager =
            await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    profile =>
                        profile.UserId ==
                        userId,
                    cancellationToken);

        if (manager?
            .OrganizationId is null)
        {
            return false;
        }

        var eligibleStatus =
            application.Status is
                ApplicationStatus
                    .Shortlisted or
                ApplicationStatus
                    .InterviewScheduled or
                ApplicationStatus
                    .Offered or
                ApplicationStatus
                    .Hired or
                ApplicationStatus
                    .Rejected;

        return eligibleStatus &&
            application.Job
                .OrganizationId ==
            manager.OrganizationId.Value &&
            (!manager.DepartmentId
                .HasValue ||
             application.Job
                .DepartmentId ==
             manager.DepartmentId);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? throw new
                UnauthorizedAccessException();
    }
}
