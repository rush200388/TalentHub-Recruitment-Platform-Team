namespace RecruitmentPlatform.Domain.Entities;

public class JobSkill
{
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public bool IsRequired { get; set; } = true;
    public decimal Weight { get; set; } = 1m;
}
