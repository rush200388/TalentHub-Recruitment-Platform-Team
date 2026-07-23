using Xunit;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Infrastructure.Matching;

namespace RecruitmentPlatform.Tests.Matching;

public sealed class JobMatchingServiceTests
{
    private readonly JobMatchingService
        _service = new();

    [Fact]
    public void Calculate_UsesSkillAndExperienceWeights()
    {
        var csharp = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "C#"
        };

        var react = new Skill
        {
            Id = Guid.NewGuid(),
            Name = "React"
        };

        var candidate =
            new CandidateProfile
            {
                YearsOfExperience = 1
            };

        candidate.CandidateSkills.Add(
            new CandidateSkill
            {
                CandidateProfile =
                    candidate,
                Skill = csharp,
                SkillId = csharp.Id
            });

        var job = new Job
        {
            MinimumExperienceYears = 2
        };

        job.JobSkills.Add(
            new JobSkill
            {
                Job = job,
                Skill = csharp,
                SkillId = csharp.Id,
                IsRequired = true,
                Weight = 1
            });

        job.JobSkills.Add(
            new JobSkill
            {
                Job = job,
                Skill = react,
                SkillId = react.Id,
                IsRequired = true,
                Weight = 1
            });

        var result =
            _service.Calculate(
                candidate,
                job);

        Assert.Equal(50m, result.Score);
        Assert.Contains(
            "C#",
            result.MatchedSkills);
        Assert.Contains(
            "React",
            result.MissingSkills);
    }

    [Fact]
    public void Calculate_NoRequiredSkills_UsesExperienceOnly()
    {
        var candidate =
            new CandidateProfile
            {
                YearsOfExperience = 3
            };

        var job =
            new Job
            {
                MinimumExperienceYears = 2
            };

        var result =
            _service.Calculate(
                candidate,
                job);

        Assert.Equal(
            100m,
            result.Score);
        Assert.Empty(
            result.MissingSkills);
    }
}
