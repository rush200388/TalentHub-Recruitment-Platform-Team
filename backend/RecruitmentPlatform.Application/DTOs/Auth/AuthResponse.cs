namespace RecruitmentPlatform.Application.DTOs.Auth;

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthUserResponse User);
