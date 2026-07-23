using Xunit;
using RecruitmentPlatform.Application.AI;
using RecruitmentPlatform.Infrastructure.AI;

namespace RecruitmentPlatform.Tests.Feedback;

public sealed class RuleBasedCandidateFeedbackServiceTests
{
    private readonly
        RuleBasedCandidateFeedbackService
        _service = new();

    [Fact]
    public void Generate_HighEvidence_ReturnsHireRecommendation()
    {
        var context =
            new CandidateFeedbackContext(
                "Software Engineer",
                2,
                3,
                88m,
                ["C#", "React"],
                [],
                82m,
                80m,
                "Hire");

        var result =
            _service.Generate(
                context,
                null);

        Assert.Contains(
            result.Recommendation,
            new[]
            {
                "Strong Hire",
                "Hire"
            });

        Assert.NotEmpty(
            result.Strengths);

        Assert.False(
            result.UsedExternalAi);
    }

    [Fact]
    public void Generate_MissingSkills_AppearsInRisks()
    {
        var context =
            new CandidateFeedbackContext(
                "Backend Developer",
                3,
                1,
                40m,
                ["C#"],
                ["Docker", "Azure"],
                null,
                null,
                null);

        var result =
            _service.Generate(
                context,
                "Test fallback");

        Assert.Contains(
            result.Risks,
            risk =>
                risk.Contains(
                    "Docker",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Equal(
            "Test fallback",
            result.FallbackReason);
    }
}
