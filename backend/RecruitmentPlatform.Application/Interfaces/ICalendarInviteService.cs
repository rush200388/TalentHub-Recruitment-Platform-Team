using RecruitmentPlatform.Application.Communication;

namespace RecruitmentPlatform.Application.Interfaces;

public interface ICalendarInviteService
{
    string BuildInterviewInvite(
        CalendarInviteRequest request);
}
