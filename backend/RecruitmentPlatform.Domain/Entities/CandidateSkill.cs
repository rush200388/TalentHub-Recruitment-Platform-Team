namespace RecruitmentPlatform.Domain.Entities;

public class CandidateSkill
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public int ProficiencyLevel { get; set; } = 1;
    public decimal? YearsOfExperience { get; set; }
}
