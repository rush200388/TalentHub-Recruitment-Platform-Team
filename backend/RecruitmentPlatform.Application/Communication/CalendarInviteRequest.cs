namespace RecruitmentPlatform.Application.Communication;

public sealed record CalendarInviteRequest(
    string EventId,
    string CandidateName,
    string CandidateEmail,
    string JobTitle,
    string InterviewerName,
    string InterviewerEmail,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    string InterviewType,
    string? MeetingLink,
    string? Location,
    string? Notes);
