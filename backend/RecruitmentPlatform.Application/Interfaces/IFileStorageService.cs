using RecruitmentPlatform.Application.Storage;

namespace RecruitmentPlatform.Application.Interfaces;

public interface IFileStorageService
{
    string ActiveProvider { get; }

    Task<StoredFileResult> SaveAsync(
        Stream source,
        string originalFileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
