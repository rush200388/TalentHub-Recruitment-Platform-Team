namespace RecruitmentPlatform.Application.DTOs.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc,
    string CreatedAgo);

public sealed record UnreadNotificationCountResponse(
    int Count);
