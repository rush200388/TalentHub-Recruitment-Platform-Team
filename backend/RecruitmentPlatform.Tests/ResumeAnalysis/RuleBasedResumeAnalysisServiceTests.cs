using Xunit;
using RecruitmentPlatform.Infrastructure.ResumeAnalysis;

namespace RecruitmentPlatform.Tests.ResumeAnalysis;

public sealed class RuleBasedResumeAnalysisServiceTests
{
    private readonly
        RuleBasedResumeAnalysisService
        _service = new();

    [Fact]
    public void Analyze_ExtractsContactSkillsAndExperience()
    {
        const string text =
            """
            Ravindu Sandeepa
            ravindu@example.com
            0771234567
            Bachelor of Software Engineering
            3 years of professional experience
            Built ASP.NET Core REST APIs using C#,
            React, PostgreSQL, Git, and Docker.
            """;

        var result =
            _service.Analyze(text);

        Assert.Equal(
            "ravindu@example.com",
            result.ExtractedEmail);

        Assert.Equal(
            "0771234567",
            result.ExtractedPhone);

        Assert.Equal(
            3,
            result
                .SuggestedYearsOfExperience);

        Assert.Contains(
            "C#",
            result.ExtractedSkills);

        Assert.Contains(
            "React",
            result.ExtractedSkills);

        Assert.Contains(
            "PostgreSQL",
            result.ExtractedSkills);

        Assert.NotEmpty(
            result.EducationSignals);
    }

    [Fact]
    public void Analyze_EmptyText_Throws()
    {
        Assert.Throws<
            ArgumentException>(
                () =>
                    _service.Analyze(
                        "   "));
    }
}
