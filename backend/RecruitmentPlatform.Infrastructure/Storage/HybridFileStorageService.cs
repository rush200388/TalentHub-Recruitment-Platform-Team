using Microsoft.Extensions.Options;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.Storage;

namespace RecruitmentPlatform.Infrastructure.Storage;

public sealed class HybridFileStorageService
    : IFileStorageService
{
    private readonly LocalFileStorageService
        _localStorage;
    private readonly AzureBlobFileStorageService
        _azureStorage;
    private readonly CloudStorageOptions _options;

    public HybridFileStorageService(
        LocalFileStorageService localStorage,
        AzureBlobFileStorageService azureStorage,
        IOptions<CloudStorageOptions> options)
    {
        _localStorage = localStorage;
        _azureStorage = azureStorage;
        _options = options.Value;
    }

    private bool UseAzure =>
        string.Equals(
            _options.Provider,
            "AzureBlob",
            StringComparison.OrdinalIgnoreCase) &&
        _azureStorage.IsConfigured;

    public string ActiveProvider =>
        UseAzure
            ? _azureStorage.ProviderName
            : _localStorage.ProviderName;

    public Task<StoredFileResult> SaveAsync(
        Stream source,
        string originalFileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken = default)
    {
        return UseAzure
            ? _azureStorage.SaveAsync(
                source,
                originalFileName,
                contentType,
                folder,
                cancellationToken)
            : _localStorage.SaveAsync(
                source,
                originalFileName,
                contentType,
                folder,
                cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        return storageKey.StartsWith(
            "azureblob://",
            StringComparison.OrdinalIgnoreCase)
            ? _azureStorage.OpenReadAsync(
                storageKey,
                cancellationToken)
            : _localStorage.OpenReadAsync(
                storageKey,
                cancellationToken);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        return storageKey.StartsWith(
            "azureblob://",
            StringComparison.OrdinalIgnoreCase)
            ? _azureStorage.DeleteAsync(
                storageKey,
                cancellationToken)
            : _localStorage.DeleteAsync(
                storageKey,
                cancellationToken);
    }
}
