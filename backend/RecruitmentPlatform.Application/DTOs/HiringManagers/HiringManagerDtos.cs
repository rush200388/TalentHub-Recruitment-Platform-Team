namespace RecruitmentPlatform.Application.DTOs.HiringManagers;

public sealed record HiringManagerResponse(
    Guid ProfileId,
    string UserId,
    string Name,
    string Email,
    Guid? OrganizationId,
    string Organization,
    Guid? DepartmentId,
    string Department,
    string? JobTitle);
