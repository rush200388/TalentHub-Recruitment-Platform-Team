using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.Communication;
using RecruitmentPlatform.Application.DTOs.Interviews;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class InterviewsController
    : ControllerBase
{
    private static readonly HashSet<string>
        AllowedRecommendations =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
                "Strong Hire",
                "Hire",
                "Consider",
                "Reject",
                "Strong Reject"
            };

    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser>
        _userManager;
    private readonly IEmailService _emailService;
    private readonly ICalendarInviteService
        _calendarInviteService;

    public InterviewsController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ICalendarInviteService
            calendarInviteService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _calendarInviteService =
            calendarInviteService;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                InterviewResponse>>> List()
    {
        var query = InterviewQuery()
            .AsNoTracking()
            .AsQueryable();

        var userId = GetCurrentUserId();

        if (User.IsInRole("Candidate"))
        {
            query = query.Where(x =>
                x.JobApplication
                    .CandidateProfile.UserId ==
                userId);
        }
        else if (User.IsInRole("Recruiter"))
        {
            query = query.Where(x =>
                x.JobApplication.Job
                    .RecruiterProfile != null &&
                x.JobApplication.Job
                    .RecruiterProfile.UserId ==
                userId);
        }
        else if (User.IsInRole("HiringManager"))
        {
            query = query.Where(x =>
                x.InterviewerUserId == userId);
        }
        else if (!User.IsInRole("Administrator"))
        {
            return Forbid();
        }

        var interviews =
            await query
                .OrderByDescending(x =>
                    x.StartTimeUtc)
                .ToListAsync();

        return Ok(
            await ToResponses(interviews));
    }

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<InterviewResponse>> Get(
            Guid id)
    {
        var interview =
            await InterviewQuery()
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.Id == id);

        if (interview is null)
        {
            return NotFound(new
            {
                message =
                    "Interview not found."
            });
        }

        if (!CanView(interview))
        {
            return Forbid();
        }

        return Ok(
            (await ToResponses([interview]))
                .Single());
    }

    [HttpPost]
    [Authorize(Roles =
        "Recruiter,Administrator")]
    public async Task<
        ActionResult<InterviewResponse>> Create(
            CreateInterviewRequest request,
            CancellationToken cancellationToken)
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

        if (!CanRecruiterManage(application))
        {
            return Forbid();
        }

        if (application.Status !=
            ApplicationStatus.Shortlisted)
        {
            return BadRequest(new
            {
                message =
                    "Only shortlisted candidates can be scheduled for an interview."
            });
        }

        var interviewer =
            await _userManager.FindByIdAsync(
                request.InterviewerUserId);

        if (interviewer is null ||
            !interviewer.IsActive ||
            !await _userManager.IsInRoleAsync(
                interviewer,
                "HiringManager"))
        {
            return BadRequest(new
            {
                message =
                    "Select an active hiring manager."
            });
        }

        var managerProfile =
            await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId ==
                    interviewer.Id);

        if (managerProfile is null)
        {
            return BadRequest(new
            {
                message =
                    "The selected user does not have a hiring-manager profile."
            });
        }

        if (!User.IsInRole("Administrator") &&
            managerProfile.OrganizationId !=
                application.Job.OrganizationId)
        {
            return BadRequest(new
            {
                message =
                    "The interviewer must belong to the job's organization."
            });
        }

        if (!TryParseType(
            request.Type,
            out var interviewType))
        {
            return BadRequest(new
            {
                message =
                    "Interview type must be Online, Onsite, or Phone."
            });
        }

        var locationError =
            ValidateLocation(
                interviewType,
                request.MeetingLink,
                request.Location);

        if (locationError is not null)
        {
            return BadRequest(new
            {
                message = locationError
            });
        }

        var conflict =
            await HasScheduleConflict(
                application.CandidateProfileId,
                interviewer.Id,
                request.StartTimeUtc,
                request.EndTimeUtc,
                null);

        if (conflict is not null)
        {
            return Conflict(new
            {
                message = conflict
            });
        }

        var interview = new Interview
        {
            JobApplicationId =
                application.Id,
            ScheduledByUserId =
                GetCurrentUserId(),
            InterviewerUserId =
                interviewer.Id,
            StartTimeUtc =
                request.StartTimeUtc
                    .ToUniversalTime(),
            EndTimeUtc =
                request.EndTimeUtc
                    .ToUniversalTime(),
            Type = interviewType,
            Status =
                InterviewStatus.Scheduled,
            MeetingLink =
                Clean(request.MeetingLink),
            Location =
                Clean(request.Location),
            Notes =
                Clean(request.Notes),
            CalendarProvider = "ICS",
            ExternalCalendarEventId =
                Guid.NewGuid().ToString("N")
        };

        _dbContext.Interviews.Add(
            interview);

        application.Status =
            ApplicationStatus
                .InterviewScheduled;

        application.Stage = "Interview";

        AddNotification(
            application
                .CandidateProfile.UserId,
            NotificationType
                .InterviewReminder,
            "Interview scheduled",
            $"Your interview for {application.Job.Title} is scheduled for {interview.StartTimeUtc:yyyy-MM-dd HH:mm} UTC.");

        AddNotification(
            interviewer.Id,
            NotificationType
                .InterviewReminder,
            "Interview assigned",
            $"You were assigned to interview {application.CandidateProfile.FirstName} {application.CandidateProfile.LastName} for {application.Job.Title}.");

        AddAudit(
            "Schedule",
            "Interview",
            interview.Id,
            $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName} - {application.Job.Title}");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await SendInvitationEmails(
            interview,
            application,
            interviewer,
            cancellationToken);

        var created =
            await InterviewQuery()
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == interview.Id,
                    cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new
            {
                id = interview.Id
            },
            (await ToResponses([created]))
                .Single());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles =
        "Recruiter,Administrator")]
    public async Task<
        ActionResult<InterviewResponse>> Update(
            Guid id,
            UpdateInterviewRequest request,
            CancellationToken cancellationToken)
    {
        var interview =
            await InterviewQuery()
                .SingleOrDefaultAsync(x =>
                    x.Id == id,
                    cancellationToken);

        if (interview is null)
        {
            return NotFound(new
            {
                message =
                    "Interview not found."
            });
        }

        if (!CanRecruiterManage(
            interview.JobApplication))
        {
            return Forbid();
        }

        if (!TryParseType(
            request.Type,
            out var interviewType))
        {
            return BadRequest(new
            {
                message =
                    "Interview type must be Online, Onsite, or Phone."
            });
        }

        if (!TryParseStatus(
            request.Status,
            out var interviewStatus) ||
            interviewStatus is not (
                InterviewStatus.Scheduled or
                InterviewStatus.Cancelled))
        {
            return BadRequest(new
            {
                message =
                    "Status must be Scheduled or Cancelled."
            });
        }

        var interviewer =
            await _userManager.FindByIdAsync(
                request.InterviewerUserId);

        if (interviewer is null ||
            !interviewer.IsActive ||
            !await _userManager.IsInRoleAsync(
                interviewer,
                "HiringManager"))
        {
            return BadRequest(new
            {
                message =
                    "Select an active hiring manager."
            });
        }

        var managerProfile =
            await _dbContext
                .HiringManagerProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(x =>
                    x.UserId ==
                    interviewer.Id);

        if (managerProfile is null)
        {
            return BadRequest(new
            {
                message =
                    "The selected user does not have a hiring-manager profile."
            });
        }

        if (!User.IsInRole("Administrator") &&
            managerProfile.OrganizationId !=
                interview.JobApplication.Job.OrganizationId)
        {
            return BadRequest(new
            {
                message =
                    "The interviewer must belong to the job's organization."
            });
        }

        var locationError =
            ValidateLocation(
                interviewType,
                request.MeetingLink,
                request.Location);

        if (locationError is not null)
        {
            return BadRequest(new
            {
                message = locationError
            });
        }

        if (interviewStatus ==
            InterviewStatus.Scheduled)
        {
            var conflict =
                await HasScheduleConflict(
                    interview.JobApplication
                        .CandidateProfileId,
                    interviewer.Id,
                    request.StartTimeUtc,
                    request.EndTimeUtc,
                    id);

            if (conflict is not null)
            {
                return Conflict(new
                {
                    message = conflict
                });
            }
        }

        interview.InterviewerUserId =
            interviewer.Id;

        interview.StartTimeUtc =
            request.StartTimeUtc
                .ToUniversalTime();

        interview.EndTimeUtc =
            request.EndTimeUtc
                .ToUniversalTime();

        interview.Type = interviewType;
        interview.Status =
            interviewStatus;
        interview.MeetingLink =
            Clean(request.MeetingLink);
        interview.Location =
            Clean(request.Location);
        interview.Notes =
            Clean(request.Notes);

        if (interviewStatus ==
            InterviewStatus.Cancelled)
        {
            interview.JobApplication.Status =
                ApplicationStatus.Shortlisted;

            interview.JobApplication.Stage =
                "Shortlist";

            AddNotification(
                interview.JobApplication
                    .CandidateProfile.UserId,
                NotificationType
                    .InterviewReminder,
                "Interview cancelled",
                $"The interview for {interview.JobApplication.Job.Title} was cancelled.");
        }
        else
        {
            interview.JobApplication.Status =
                ApplicationStatus
                    .InterviewScheduled;

            interview.JobApplication.Stage =
                "Interview";

            AddNotification(
                interview.JobApplication
                    .CandidateProfile.UserId,
                NotificationType
                    .InterviewReminder,
                "Interview updated",
                $"Your interview for {interview.JobApplication.Job.Title} is scheduled for {interview.StartTimeUtc:yyyy-MM-dd HH:mm} UTC.");
        }

        AddAudit(
            interviewStatus ==
                InterviewStatus.Cancelled
                ? "Cancel"
                : "Reschedule",
            "Interview",
            interview.Id,
            interview.JobApplication.Job.Title);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (interviewStatus ==
            InterviewStatus.Scheduled)
        {
            await SendInvitationEmails(
                interview,
                interview.JobApplication,
                interviewer,
                cancellationToken);
        }

        return Ok(
            (await ToResponses([interview]))
                .Single());
    }

    [HttpPost("{id:guid}/feedback")]
    [Authorize(Roles =
        "HiringManager,Administrator")]
    public async Task<
        ActionResult<InterviewResponse>>
        SubmitFeedback(
            Guid id,
            SubmitInterviewFeedbackRequest request,
            CancellationToken cancellationToken)
    {
        var interview =
            await InterviewQuery()
                .SingleOrDefaultAsync(x =>
                    x.Id == id,
                    cancellationToken);

        if (interview is null)
        {
            return NotFound(new
            {
                message =
                    "Interview not found."
            });
        }

        var userId = GetCurrentUserId();

        if (!User.IsInRole("Administrator") &&
            interview.InterviewerUserId !=
                userId)
        {
            return Forbid();
        }

        if (interview.Status ==
            InterviewStatus.Cancelled)
        {
            return BadRequest(new
            {
                message =
                    "Feedback cannot be added to a cancelled interview."
            });
        }

        if (!AllowedRecommendations.Contains(
            request.Recommendation))
        {
            return BadRequest(new
            {
                message =
                    "Recommendation must be Strong Hire, Hire, Consider, Reject, or Strong Reject."
            });
        }

        var feedback =
            interview.Feedback
                .SingleOrDefault(x =>
                    x.ReviewerUserId ==
                    userId);

        if (feedback is null)
        {
            feedback =
                new InterviewFeedback
                {
                    InterviewId =
                        interview.Id,
                    ReviewerUserId =
                        userId
                };

            _dbContext
                .InterviewFeedback
                .Add(feedback);
        }

        feedback.OverallRating =
            request.OverallRating;
        feedback.TechnicalScore =
            request.TechnicalScore;
        feedback.CommunicationScore =
            request.CommunicationScore;
        feedback.CultureFitScore =
            request.CultureFitScore;
        feedback.Comments =
            Clean(request.Comments);
        feedback.Recommendation =
            request.Recommendation.Trim();

        interview.Status =
            InterviewStatus.Completed;

        interview.JobApplication.Stage =
            "Interview Completed";

        AddNotification(
            interview.JobApplication
                .CandidateProfile.UserId,
            NotificationType
                .ApplicationUpdate,
            "Interview completed",
            $"Your interview for {interview.JobApplication.Job.Title} has been completed.");

        AddAudit(
            "SubmitFeedback",
            "Interview",
            interview.Id,
            $"{interview.JobApplication.CandidateProfile.FirstName} {interview.JobApplication.CandidateProfile.LastName}");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var updated =
            await InterviewQuery()
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == id,
                    cancellationToken);

        return Ok(
            (await ToResponses([updated]))
                .Single());
    }

    private IQueryable<Interview>
        InterviewQuery()
    {
        return _dbContext.Interviews
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.CandidateProfile)
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.Job)
                    .ThenInclude(x =>
                        x.Organization)
            .Include(x =>
                x.JobApplication)
                .ThenInclude(x =>
                    x.Job)
                    .ThenInclude(x =>
                        x.RecruiterProfile)
            .Include(x =>
                x.Feedback);
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
                    x.Organization)
            .Include(x =>
                x.Job)
                .ThenInclude(x =>
                    x.RecruiterProfile);
    }

    private bool CanView(
        Interview interview)
    {
        if (User.IsInRole(
            "Administrator"))
        {
            return true;
        }

        var userId =
            GetCurrentUserId();

        if (User.IsInRole("Candidate"))
        {
            return interview.JobApplication
                .CandidateProfile.UserId ==
                userId;
        }

        if (User.IsInRole("Recruiter"))
        {
            return interview.JobApplication
                .Job.RecruiterProfile
                ?.UserId == userId;
        }

        return User.IsInRole(
                "HiringManager") &&
            interview.InterviewerUserId ==
                userId;
    }

    private bool CanRecruiterManage(
        JobApplication application)
    {
        if (User.IsInRole(
            "Administrator"))
        {
            return true;
        }

        return User.IsInRole(
                "Recruiter") &&
            application.Job
                .RecruiterProfile
                ?.UserId ==
            GetCurrentUserId();
    }

    private async Task<string?>
        HasScheduleConflict(
            Guid candidateProfileId,
            string interviewerUserId,
            DateTime startTimeUtc,
            DateTime endTimeUtc,
            Guid? excludedInterviewId)
    {
        var start =
            startTimeUtc.ToUniversalTime();

        var end =
            endTimeUtc.ToUniversalTime();

        var interviewerConflict =
            await _dbContext.Interviews
                .AnyAsync(x =>
                    (!excludedInterviewId.HasValue ||
                     x.Id !=
                        excludedInterviewId.Value) &&
                    x.Status ==
                        InterviewStatus
                            .Scheduled &&
                    x.InterviewerUserId ==
                        interviewerUserId &&
                    start <
                        x.EndTimeUtc &&
                    end >
                        x.StartTimeUtc);

        if (interviewerConflict)
        {
            return
                "The hiring manager already has another interview during this time.";
        }

        var candidateConflict =
            await _dbContext.Interviews
                .AnyAsync(x =>
                    (!excludedInterviewId.HasValue ||
                     x.Id !=
                        excludedInterviewId.Value) &&
                    x.Status ==
                        InterviewStatus
                            .Scheduled &&
                    x.JobApplication
                        .CandidateProfileId ==
                        candidateProfileId &&
                    start <
                        x.EndTimeUtc &&
                    end >
                        x.StartTimeUtc);

        return candidateConflict
            ? "The candidate already has another interview during this time."
            : null;
    }

    private async Task SendInvitationEmails(
        Interview interview,
        JobApplication application,
        ApplicationUser interviewer,
        CancellationToken cancellationToken)
    {
        var candidateUser =
            await _userManager.FindByIdAsync(
                application
                    .CandidateProfile
                    .UserId);

        if (candidateUser?.Email is null ||
            interviewer.Email is null)
        {
            return;
        }

        var candidateName =
            $"{application.CandidateProfile.FirstName} {application.CandidateProfile.LastName}"
                .Trim();

        var interviewerName =
            $"{interviewer.FirstName} {interviewer.LastName}"
                .Trim();

        var inviteRequest =
            new CalendarInviteRequest(
                interview
                    .ExternalCalendarEventId
                    ?? interview.Id.ToString(),
                candidateName,
                candidateUser.Email,
                application.Job.Title,
                interviewerName,
                interviewer.Email,
                interview.StartTimeUtc,
                interview.EndTimeUtc,
                FormatType(interview.Type),
                interview.MeetingLink,
                interview.Location,
                interview.Notes);

        var calendarInvite =
            _calendarInviteService
                .BuildInterviewInvite(
                    inviteRequest);

        var attachment =
            new EmailAttachmentData(
                "interview-invitation.ics",
                "text/calendar; charset=utf-8; method=REQUEST",
                Encoding.UTF8.GetBytes(
                    calendarInvite));

        var details =
            BuildInterviewEmailBody(
                candidateName,
                application.Job.Title,
                interviewerName,
                interview);

        await _emailService.SendAsync(
            new EmailMessage(
                candidateUser.Email,
                $"Interview scheduled - {application.Job.Title}",
                details,
                [attachment]),
            cancellationToken);

        var managerDetails =
            BuildManagerEmailBody(
                candidateName,
                application.Job.Title,
                interviewerName,
                interview);

        await _emailService.SendAsync(
            new EmailMessage(
                interviewer.Email,
                $"Interview assignment - {candidateName}",
                managerDetails,
                [attachment]),
            cancellationToken);
    }

    private static string
        BuildInterviewEmailBody(
            string candidateName,
            string jobTitle,
            string interviewerName,
            Interview interview)
    {
        return $"""
            <h2>Interview Scheduled</h2>
            <p>Hello {WebUtility.HtmlEncode(candidateName)},</p>
            <p>Your interview for <strong>{WebUtility.HtmlEncode(jobTitle)}</strong> has been scheduled.</p>
            <ul>
              <li><strong>Start:</strong> {interview.StartTimeUtc:yyyy-MM-dd HH:mm} UTC</li>
              <li><strong>End:</strong> {interview.EndTimeUtc:yyyy-MM-dd HH:mm} UTC</li>
              <li><strong>Type:</strong> {WebUtility.HtmlEncode(FormatType(interview.Type))}</li>
              <li><strong>Interviewer:</strong> {WebUtility.HtmlEncode(interviewerName)}</li>
              <li><strong>Meeting link/location:</strong> {WebUtility.HtmlEncode(interview.MeetingLink ?? interview.Location ?? "To be confirmed")}</li>
            </ul>
            <p>A calendar invitation is attached.</p>
            """;
    }

    private static string
        BuildManagerEmailBody(
            string candidateName,
            string jobTitle,
            string interviewerName,
            Interview interview)
    {
        return $"""
            <h2>Interview Assignment</h2>
            <p>Hello {WebUtility.HtmlEncode(interviewerName)},</p>
            <p>You have been assigned to interview <strong>{WebUtility.HtmlEncode(candidateName)}</strong> for <strong>{WebUtility.HtmlEncode(jobTitle)}</strong>.</p>
            <ul>
              <li><strong>Start:</strong> {interview.StartTimeUtc:yyyy-MM-dd HH:mm} UTC</li>
              <li><strong>End:</strong> {interview.EndTimeUtc:yyyy-MM-dd HH:mm} UTC</li>
              <li><strong>Type:</strong> {WebUtility.HtmlEncode(FormatType(interview.Type))}</li>
            </ul>
            <p>A calendar invitation is attached.</p>
            """;
    }

    private async Task<
        IReadOnlyCollection<
            InterviewResponse>> ToResponses(
                IReadOnlyCollection<
                    Interview> interviews)
    {
        var userIds = interviews
            .SelectMany(x =>
                new[]
                {
                    x.InterviewerUserId,
                    x.ScheduledByUserId,
                    x.JobApplication
                        .CandidateProfile
                        .UserId
                }
                .Concat(
                    x.Feedback.Select(
                        feedback =>
                            feedback.ReviewerUserId)))
            .Where(x =>
                !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct()
            .ToArray();

        var users =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id);

        return interviews
            .Select(interview =>
            {
                users.TryGetValue(
                    interview
                        .InterviewerUserId
                        ?? string.Empty,
                    out var interviewer);

                users.TryGetValue(
                    interview
                        .ScheduledByUserId,
                    out var scheduler);

                users.TryGetValue(
                    interview
                        .JobApplication
                        .CandidateProfile
                        .UserId,
                    out var candidateUser);

                var feedback =
                    interview.Feedback
                        .OrderByDescending(x =>
                            x.UpdatedAtUtc ??
                            x.CreatedAtUtc)
                        .FirstOrDefault();

                users.TryGetValue(
                    feedback
                        ?.ReviewerUserId
                        ?? string.Empty,
                    out var reviewer);

                return new InterviewResponse(
                    interview.Id,
                    interview.JobApplicationId,
                    interview.JobApplication
                        .CandidateProfileId,
                    $"{interview.JobApplication.CandidateProfile.FirstName} {interview.JobApplication.CandidateProfile.LastName}"
                        .Trim(),
                    candidateUser?.Email
                        ?? string.Empty,
                    interview.JobApplication
                        .JobId,
                    interview.JobApplication
                        .Job.Title,
                    interview.JobApplication
                        .Job.Organization.Name,
                    interview.InterviewerUserId
                        ?? string.Empty,
                    UserName(interviewer),
                    interviewer?.Email
                        ?? string.Empty,
                    interview
                        .ScheduledByUserId,
                    UserName(scheduler),
                    interview.StartTimeUtc,
                    interview.EndTimeUtc,
                    interview.StartTimeUtc
                        .ToString(
                            "yyyy-MM-dd"),
                    interview.StartTimeUtc
                        .ToString(
                            "HH:mm"),
                    interview.EndTimeUtc
                        .ToString(
                            "HH:mm"),
                    FormatType(
                        interview.Type),
                    interview.Status
                        .ToString(),
                    interview.MeetingLink,
                    interview.Location,
                    interview.Notes,
                    feedback is null
                        ? null
                        : new InterviewFeedbackResponse(
                            feedback.Id,
                            feedback
                                .ReviewerUserId,
                            UserName(reviewer),
                            feedback
                                .OverallRating,
                            feedback
                                .TechnicalScore,
                            feedback
                                .CommunicationScore,
                            feedback
                                .CultureFitScore,
                            feedback.Comments,
                            feedback
                                .Recommendation
                                ?? string.Empty,
                            feedback
                                .UpdatedAtUtc
                                ?? feedback
                                    .CreatedAtUtc));
            })
            .ToList();
    }

    private static string UserName(
        ApplicationUser? user)
    {
        if (user is null)
        {
            return "Unknown user";
        }

        return
            $"{user.FirstName} {user.LastName}"
                .Trim();
    }

    private static bool TryParseType(
        string value,
        out InterviewType type)
    {
        var normalized =
            value.Replace(
                "-",
                string.Empty)
            .Replace(
                " ",
                string.Empty);

        return Enum.TryParse(
            normalized,
            true,
            out type);
    }

    private static bool TryParseStatus(
        string value,
        out InterviewStatus status)
    {
        return Enum.TryParse(
            value.Replace(
                " ",
                string.Empty),
            true,
            out status);
    }

    private static string? ValidateLocation(
        InterviewType type,
        string? meetingLink,
        string? location)
    {
        if (type ==
            InterviewType.Online)
        {
            if (!Uri.TryCreate(
                    meetingLink,
                    UriKind.Absolute,
                    out var uri) ||
                (uri.Scheme !=
                    Uri.UriSchemeHttp &&
                 uri.Scheme !=
                    Uri.UriSchemeHttps))
            {
                return
                    "Online interviews require a valid meeting link beginning with http:// or https://.";
            }
        }

        if (type ==
                InterviewType.Onsite &&
            string.IsNullOrWhiteSpace(
                location))
        {
            return
                "Onsite interviews require a physical location.";
        }

        return null;
    }

    private static string FormatType(
        InterviewType type)
    {
        return type switch
        {
            InterviewType.Onsite =>
                "Onsite",
            InterviewType.Online =>
                "Online",
            _ => "Phone"
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

    private void AddNotification(
        string userId,
        NotificationType type,
        string title,
        string message)
    {
        _dbContext.Notifications.Add(
            new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message
            });
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
