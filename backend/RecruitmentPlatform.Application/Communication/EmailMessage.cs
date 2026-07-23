namespace RecruitmentPlatform.Application.Communication;

public sealed record EmailAttachmentData(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    IReadOnlyCollection<EmailAttachmentData> Attachments);
