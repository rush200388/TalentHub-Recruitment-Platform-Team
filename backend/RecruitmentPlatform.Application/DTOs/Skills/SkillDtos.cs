namespace RecruitmentPlatform.Application.DTOs.Skills;

public sealed record SkillResponse(
    Guid Id,
    string Name,
    string? Category);
