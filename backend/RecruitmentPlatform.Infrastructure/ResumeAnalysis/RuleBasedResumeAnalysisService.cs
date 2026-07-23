using System.Text.RegularExpressions;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.ResumeAnalysis;

namespace RecruitmentPlatform.Infrastructure.ResumeAnalysis;

public sealed class RuleBasedResumeAnalysisService
    : IResumeAnalysisService
{
    private static readonly Regex EmailRegex = new(
        @"(?<![\w.+-])[\w.+-]+@[\w-]+(?:\.[\w-]+)+(?![\w.-])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LocalPhoneRegex = new(
        @"(?<!\d)0\d{9}(?!\d)",
        RegexOptions.Compiled);

    private static readonly Regex InternationalPhoneRegex = new(
        @"(?<!\d)\+94[\s-]?\d{2}[\s-]?\d{3}[\s-]?\d{4}(?!\d)",
        RegexOptions.Compiled);

    private static readonly Regex ExperienceYearsRegex = new(
        @"(?<years>\d{1,2})\s*\+?\s*(?:years?|yrs?)\s+(?:of\s+)?(?:professional\s+)?experience",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly IReadOnlyDictionary<string, string[]> SkillAliases =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["C#"] = ["c#", "c sharp", ".net", "asp.net", "asp.net core"],
            ["Java"] = ["java", "spring boot", "spring framework"],
            ["JavaScript"] = ["javascript", "ecmascript"],
            ["TypeScript"] = ["typescript"],
            ["React"] = ["react", "react.js", "reactjs"],
            ["Angular"] = ["angular"],
            ["Vue.js"] = ["vue", "vue.js", "vuejs"],
            ["Node.js"] = ["node.js", "nodejs", "express.js", "expressjs"],
            ["Python"] = ["python", "django", "flask", "fastapi"],
            ["PHP"] = ["php", "laravel"],
            ["HTML"] = ["html", "html5"],
            ["CSS"] = ["css", "css3", "bootstrap", "tailwind"],
            ["SQL"] = ["sql", "structured query language"],
            ["PostgreSQL"] = ["postgresql", "postgres"],
            ["MySQL"] = ["mysql"],
            ["SQL Server"] = ["sql server", "mssql"],
            ["MongoDB"] = ["mongodb", "mongo db"],
            ["REST API"] = ["rest api", "restful api", "web api"],
            ["GraphQL"] = ["graphql"],
            ["Git"] = ["git", "github", "gitlab", "bitbucket"],
            ["Docker"] = ["docker", "containerization", "containers"],
            ["Kubernetes"] = ["kubernetes", "k8s"],
            ["Azure"] = ["microsoft azure", "azure"],
            ["AWS"] = ["amazon web services", "aws"],
            ["Google Cloud"] = ["google cloud", "gcp"],
            ["CI/CD"] = ["ci/cd", "continuous integration", "continuous delivery"],
            ["Agile"] = ["agile", "scrum", "kanban"],
            ["Unit Testing"] = ["unit testing", "unit tests", "xunit", "nunit", "jest"],
            ["Entity Framework"] = ["entity framework", "ef core"],
            ["JWT"] = ["jwt", "json web token"],
            ["Linux"] = ["linux", "ubuntu"],
            ["Figma"] = ["figma"],
            ["Power BI"] = ["power bi", "powerbi"],
            ["Machine Learning"] = ["machine learning", "ml model", "scikit-learn"],
            ["Artificial Intelligence"] = ["artificial intelligence", "generative ai", "large language model", "llm"],
            ["Data Analysis"] = ["data analysis", "data analytics"],
            ["Communication"] = ["communication skills", "written communication", "verbal communication"],
            ["Leadership"] = ["leadership", "team lead", "team leader"],
            ["Problem Solving"] = ["problem solving", "problem-solving"]
        };

    private static readonly string[] EducationKeywords =
    [
        "bachelor",
        "bsc",
        "b.sc",
        "master",
        "msc",
        "m.sc",
        "diploma",
        "degree",
        "university",
        "college",
        "higher national diploma",
        "hnd",
        "phd",
        "doctorate",
        "certification",
        "certificate"
    ];

    private static readonly string[] ExperienceKeywords =
    [
        "work experience",
        "professional experience",
        "employment history",
        "internship",
        "intern",
        "software engineer",
        "software developer",
        "project",
        "responsibilities",
        "achievements",
        "worked at",
        "developed",
        "implemented",
        "designed",
        "managed",
        "led"
    ];

    public string StrategyName =>
        "RuleBasedKeywordAndPatternStrategy-v1";

    public ResumeAnalysisResult Analyze(string resumeText)
    {
        if (string.IsNullOrWhiteSpace(resumeText))
        {
            throw new ArgumentException(
                "Resume text is required.",
                nameof(resumeText));
        }

        var normalized = resumeText.ToLowerInvariant();
        var lines = resumeText
            .Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();

        var extractedSkills = SkillAliases
            .Where(pair =>
                pair.Value.Any(alias =>
                    ContainsAlias(normalized, alias)))
            .Select(pair => pair.Key)
            .OrderBy(skill => skill)
            .ToArray();

        var educationSignals = lines
            .Where(line =>
                EducationKeywords.Any(keyword =>
                    line.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();

        var experienceSignals = lines
            .Where(line =>
                ExperienceKeywords.Any(keyword =>
                    line.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToArray();

        var years = ExperienceYearsRegex
            .Matches(resumeText)
            .Select(match =>
                int.TryParse(
                    match.Groups["years"].Value,
                    out var parsed)
                    ? parsed
                    : 0)
            .Where(value => value is >= 0 and <= 60)
            .DefaultIfEmpty()
            .Max();

        var wordCount = Regex.Matches(
            resumeText,
            @"\b[\p{L}\p{N}][\p{L}\p{N}+#.-]*\b")
            .Count;

        var email = EmailRegex.Match(resumeText);
        var localPhone = LocalPhoneRegex.Match(resumeText);
        var internationalPhone = InternationalPhoneRegex.Match(resumeText);

        var warnings = new List<string>();

        if (wordCount < 80)
        {
            warnings.Add(
                "Very little readable text was extracted. The resume may be incomplete or image-based.");
        }

        if (extractedSkills.Length == 0)
        {
            warnings.Add(
                "No known technical or professional skills were detected.");
        }

        if (!email.Success)
        {
            warnings.Add(
                "No email address was detected in the resume.");
        }

        if (!localPhone.Success && !internationalPhone.Success)
        {
            warnings.Add(
                "No phone number was detected in the resume.");
        }

        if (educationSignals.Length == 0)
        {
            warnings.Add(
                "No clear education section or qualification was detected.");
        }

        return new ResumeAnalysisResult(
            wordCount,
            email.Success ? email.Value : null,
            localPhone.Success
                ? localPhone.Value
                : internationalPhone.Success
                    ? internationalPhone.Value
                    : null,
            years > 0 ? years : null,
            extractedSkills,
            educationSignals,
            experienceSignals,
            warnings);
    }

    private static bool ContainsAlias(
        string normalizedText,
        string alias)
    {
        var escaped = Regex.Escape(alias.ToLowerInvariant());

        var pattern =
            $@"(?<![\p{{L}}\p{{N}}]){escaped}(?![\p{{L}}\p{{N}}])";

        return Regex.IsMatch(
            normalizedText,
            pattern,
            RegexOptions.IgnoreCase);
    }
}
