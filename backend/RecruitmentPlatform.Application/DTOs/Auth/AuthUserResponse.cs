namespace RecruitmentPlatform.Application.DTOs.Auth;

public sealed record AuthUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyCollection<string> Roles);
