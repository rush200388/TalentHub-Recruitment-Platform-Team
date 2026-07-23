using System.Text;
using RecruitmentPlatform.Application.Communication;
using RecruitmentPlatform.Application.Interfaces;

namespace RecruitmentPlatform.Infrastructure.Communication;

public sealed class CalendarInviteService
    : ICalendarInviteService
{
    public string BuildInterviewInvite(
        CalendarInviteRequest request)
    {
        var location = !string.IsNullOrWhiteSpace(
            request.MeetingLink)
            ? request.MeetingLink
            : request.Location ?? string.Empty;

        var description = string.Join(
            "\\n",
            new[]
            {
                $"Interview for {request.JobTitle}",
                $"Candidate: {request.CandidateName}",
                $"Interviewer: {request.InterviewerName}",
                $"Type: {request.InterviewType}",
                request.Notes ?? string.Empty
            }.Where(x =>
                !string.IsNullOrWhiteSpace(x)));

        var builder = new StringBuilder();

        builder.AppendLine("BEGIN:VCALENDAR");
        builder.AppendLine("VERSION:2.0");
        builder.AppendLine("PRODID:-//TalentHub//Recruitment Platform//EN");
        builder.AppendLine("CALSCALE:GREGORIAN");
        builder.AppendLine("METHOD:REQUEST");
        builder.AppendLine("BEGIN:VEVENT");
        builder.AppendLine(
            $"UID:{Escape(request.EventId)}@talenthub");
        builder.AppendLine(
            $"DTSTAMP:{FormatUtc(DateTime.UtcNow)}");
        builder.AppendLine(
            $"DTSTART:{FormatUtc(request.StartTimeUtc)}");
        builder.AppendLine(
            $"DTEND:{FormatUtc(request.EndTimeUtc)}");
        builder.AppendLine(
            $"SUMMARY:{Escape($"Interview - {request.JobTitle}")}");
        builder.AppendLine(
            $"DESCRIPTION:{Escape(description)}");
        builder.AppendLine(
            $"LOCATION:{Escape(location)}");
        builder.AppendLine(
            $"ORGANIZER;CN={Escape(request.InterviewerName)}:MAILTO:{request.InterviewerEmail}");
        builder.AppendLine(
            $"ATTENDEE;CN={Escape(request.CandidateName)};RSVP=TRUE:MAILTO:{request.CandidateEmail}");
        builder.AppendLine("STATUS:CONFIRMED");
        builder.AppendLine("SEQUENCE:0");
        builder.AppendLine("END:VEVENT");
        builder.AppendLine("END:VCALENDAR");

        return builder.ToString();
    }

    private static string FormatUtc(DateTime value)
    {
        return value.ToUniversalTime()
            .ToString("yyyyMMdd'T'HHmmss'Z'");
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\r\n", "\\n")
            .Replace("\n", "\\n");
    }
}
