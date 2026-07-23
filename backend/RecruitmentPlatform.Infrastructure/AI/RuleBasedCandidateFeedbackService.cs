using RecruitmentPlatform.Application.AI;
using RecruitmentPlatform.Application.Interfaces;

namespace RecruitmentPlatform.Infrastructure.AI;

public sealed class RuleBasedCandidateFeedbackService
    : ICandidateFeedbackService
{
    public Task<CandidateFeedbackResult> GenerateAsync(
        CandidateFeedbackContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        return Task.FromResult(
            Generate(
                context,
                null));
    }

    public CandidateFeedbackResult Generate(
        CandidateFeedbackContext context,
        string? fallbackReason)
    {
        var strengths = new List<string>();
        var risks = new List<string>();

        if (context.MatchScore >= 75)
        {
            strengths.Add(
                $"Strong job compatibility with a {context.MatchScore:0}% match score.");
        }
        else if (context.MatchScore >= 55)
        {
            strengths.Add(
                $"Moderate job compatibility with a {context.MatchScore:0}% match score.");
        }
        else
        {
            risks.Add(
                $"Current job match is {context.MatchScore:0}%, which is below the preferred screening threshold.");
        }

        if (context.MatchedSkills.Count > 0)
        {
            strengths.Add(
                $"Matched required skills: {string.Join(", ", context.MatchedSkills)}.");
        }

        if (context.MissingSkills.Count > 0)
        {
            risks.Add(
                $"Missing or unverified required skills: {string.Join(", ", context.MissingSkills)}.");
        }

        if (context.CandidateExperienceYears >=
            context.RequiredExperienceYears)
        {
            strengths.Add(
                $"Experience meets the requirement ({context.CandidateExperienceYears} years supplied; {context.RequiredExperienceYears} required).");
        }
        else
        {
            risks.Add(
                $"Experience is below the requirement by {context.RequiredExperienceYears - context.CandidateExperienceYears} year(s).");
        }

        if (context.EvaluationScore.HasValue)
        {
            if (context.EvaluationScore >= 70)
            {
                strengths.Add(
                    $"Formal evaluation score is {context.EvaluationScore:0}%.");
            }
            else
            {
                risks.Add(
                    $"Formal evaluation score is {context.EvaluationScore:0}% and needs review.");
            }
        }
        else
        {
            risks.Add(
                "No formal candidate evaluation has been recorded.");
        }

        if (context.InterviewScore.HasValue)
        {
            if (context.InterviewScore >= 70)
            {
                strengths.Add(
                    $"Interview evidence is positive at {context.InterviewScore:0}%.");
            }
            else
            {
                risks.Add(
                    $"Interview evidence is currently {context.InterviewScore:0}%.");
            }
        }
        else
        {
            risks.Add(
                "No completed interview rating is available.");
        }

        var combinedScore =
            CalculateCombinedScore(context);

        var recommendation =
            combinedScore switch
            {
                >= 85 => "Strong Hire",
                >= 70 => "Hire",
                >= 55 => "Consider",
                >= 40 => "Reject",
                _ => "Strong Reject"
            };

        var summary =
            $"{context.JobTitle}: combined evidence score {combinedScore:0}%. " +
            $"Recommendation: {recommendation}.";

        var suggestedFeedback =
            BuildSuggestedFeedback(
                context,
                recommendation);

        return new CandidateFeedbackResult(
            "RuleBasedFallback",
            false,
            summary,
            strengths,
            risks,
            recommendation,
            suggestedFeedback,
            DateTime.UtcNow,
            fallbackReason);
    }

    private static decimal CalculateCombinedScore(
        CandidateFeedbackContext context)
    {
        var evidence = new List<
            (decimal Value, decimal Weight)>
        {
            (
                Math.Clamp(
                    context.MatchScore,
                    0m,
                    100m),
                0.50m)
        };

        if (context.EvaluationScore.HasValue)
        {
            evidence.Add((
                Math.Clamp(
                    context.EvaluationScore.Value,
                    0m,
                    100m),
                0.30m));
        }

        if (context.InterviewScore.HasValue)
        {
            evidence.Add((
                Math.Clamp(
                    context.InterviewScore.Value,
                    0m,
                    100m),
                0.20m));
        }

        var totalWeight =
            evidence.Sum(x => x.Weight);

        return Math.Round(
            evidence.Sum(
                x => x.Value * x.Weight) /
            totalWeight,
            2);
    }

    private static string BuildSuggestedFeedback(
        CandidateFeedbackContext context,
        string recommendation)
    {
        var strengths =
            context.MatchedSkills.Count > 0
                ? $"Your relevant strengths include {string.Join(", ", context.MatchedSkills)}."
                : "Your application contains relevant experience for further review.";

        var development =
            context.MissingSkills.Count > 0
                ? $"To strengthen future applications, provide clearer evidence for {string.Join(", ", context.MissingSkills)}."
                : "Continue providing clear evidence and examples of your skills.";

        return
            $"Thank you for applying for the {context.JobTitle} position. " +
            $"{strengths} {development} " +
            $"The current internal recommendation is {recommendation}. " +
            "A human reviewer must confirm the final decision.";
    }
}
