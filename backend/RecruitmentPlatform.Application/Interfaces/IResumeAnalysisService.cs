using RecruitmentPlatform.Application.ResumeAnalysis;

namespace RecruitmentPlatform.Application.Interfaces;

public interface IResumeAnalysisService
{
    string StrategyName { get; }

    ResumeAnalysisResult Analyze(string resumeText);
}
