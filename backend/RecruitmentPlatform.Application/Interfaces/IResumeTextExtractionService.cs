namespace RecruitmentPlatform.Application.Interfaces;

public interface IResumeTextExtractionService
{
    Task<string> ExtractTextAsync(
        string storagePath,
        string originalFileName,
        CancellationToken cancellationToken = default);
}
