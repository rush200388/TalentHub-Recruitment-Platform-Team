using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecruitmentPlatform.Application.Communication;
using RecruitmentPlatform.Application.Interfaces;

namespace RecruitmentPlatform.Infrastructure.Communication;

public sealed class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IConfiguration configuration,
        ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        var enabled = bool.TryParse(
            _configuration["Email:Enabled"],
            out var parsedEnabled)
            && parsedEnabled;

        if (!enabled)
        {
            _logger.LogInformation(
                "Email delivery is disabled. Skipping email to {Recipient}.",
                message.To);
            return;
        }

        var host = _configuration["Email:Host"];
        var fromEmail = _configuration["Email:FromEmail"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogWarning(
                "Email is enabled but Email:Host or Email:FromEmail is missing.");
            return;
        }

        var port = int.TryParse(
            _configuration["Email:Port"],
            out var parsedPort)
            ? parsedPort
            : 587;

        var enableSsl = !bool.TryParse(
                _configuration["Email:EnableSsl"],
                out var parsedSsl)
            || parsedSsl;

        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromName =
            _configuration["Email:FromName"]
            ?? "TalentHub Recruitment";

        try
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    fromEmail,
                    fromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(message.To);

            foreach (var attachmentData in message.Attachments)
            {
                var stream = new MemoryStream(
                    attachmentData.Content,
                    writable: false);

                mailMessage.Attachments.Add(
                    new Attachment(
                        stream,
                        attachmentData.FileName,
                        attachmentData.ContentType));
            }

            using var smtpClient = new SmtpClient(
                host,
                port)
            {
                EnableSsl = enableSsl
            };

            if (!string.IsNullOrWhiteSpace(username))
            {
                smtpClient.Credentials =
                    new NetworkCredential(
                        username,
                        password);
            }
            else
            {
                smtpClient.UseDefaultCredentials = true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation(
                "Email sent to {Recipient}.",
                message.To);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Email delivery to {Recipient} failed. Internal notification remains available.",
                message.To);
        }
    }
}
