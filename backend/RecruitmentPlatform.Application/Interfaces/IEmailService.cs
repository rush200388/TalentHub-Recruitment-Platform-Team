using RecruitmentPlatform.Application.Communication;

namespace RecruitmentPlatform.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
