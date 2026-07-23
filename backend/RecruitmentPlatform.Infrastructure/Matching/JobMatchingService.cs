using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.Matching;
using RecruitmentPlatform.Domain.Entities;

namespace RecruitmentPlatform.Infrastructure.Matching;

public sealed class JobMatchingService : IJobMatchingService
{
    public JobMatchResult Calculate(
        CandidateProfile candidate,
        Job job)
    {
        var candidateSkills = candidate.CandidateSkills
            .Select(x => x.Skill.Name.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requiredSkills = job.JobSkills
            .Where(x => x.IsRequired)
            .ToList();

        var matchedSkills = requiredSkills
            .Where(x => candidateSkills.Contains(x.Skill.Name))
            .Select(x => x.Skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        var missingSkills = requiredSkills
            .Where(x => !candidateSkills.Contains(x.Skill.Name))
            .Select(x => x.Skill.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        var totalWeight = requiredSkills.Sum(
            x => x.Weight <= 0 ? 1m : x.Weight);

        var matchedWeight = requiredSkills
            .Where(x => candidateSkills.Contains(x.Skill.Name))
            .Sum(x => x.Weight <= 0 ? 1m : x.Weight);

        var skillScore = totalWeight == 0
            ? 100m
            : matchedWeight / totalWeight * 100m;

        var experienceScore = job.MinimumExperienceYears <= 0
            ? 100m
            : Math.Min(
                100m,
                candidate.YearsOfExperience * 100m /
                job.MinimumExperienceYears);

        var finalScore = totalWeight == 0
            ? experienceScore
            : skillScore * 0.80m + experienceScore * 0.20m;

        finalScore = Math.Round(
            Math.Clamp(finalScore, 0m, 100m),
            2);

        var reason = totalWeight == 0
            ? $"Experience compatibility: {experienceScore:0}%."
            : $"Matched {matchedSkills.Length} of {requiredSkills.Count} required skills; experience compatibility: {experienceScore:0}%.";

        return new JobMatchResult(
            finalScore,
            matchedSkills,
            missingSkills,
            reason);
    }
}
