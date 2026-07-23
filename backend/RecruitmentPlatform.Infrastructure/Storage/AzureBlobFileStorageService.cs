using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using RecruitmentPlatform.Application.Storage;

namespace RecruitmentPlatform.Infrastructure.Storage;

public sealed class AzureBlobFileStorageService
{
    private const string Prefix = "azureblob://";
    private readonly CloudStorageOptions _options;

    public AzureBlobFileStorageService(
        IOptions<CloudStorageOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(
            _options.AzureConnectionString) &&
        !string.IsNullOrWhiteSpace(
            _options.AzureContainer);

    public string ProviderName =>
        "Azure Blob Storage";

    public async Task<StoredFileResult> SaveAsync(
        Stream source,
        string originalFileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken)
    {
        var container =
            CreateContainerClient();

        await container.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken:
                cancellationToken);

        var extension =
            Path.GetExtension(originalFileName)
                .ToLowerInvariant();

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var blobName =
            $"{NormalizeFolder(folder)}/{storedFileName}";

        var blob =
            container.GetBlobClient(blobName);

        await blob.UploadAsync(
            source,
            new BlobUploadOptions
            {
                HttpHeaders =
                    new BlobHttpHeaders
                    {
                        ContentType =
                            string.IsNullOrWhiteSpace(
                                contentType)
                                ? "application/octet-stream"
                                : contentType
                    }
            },
            cancellationToken);

        return new StoredFileResult(
            $"{Prefix}{container.Name}/{blobName}",
            storedFileName,
            ProviderName);
    }

    public async Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        var parsed = Parse(storageKey);

        if (parsed is null)
        {
            return null;
        }

        var container =
            new BlobContainerClient(
                _options.AzureConnectionString,
                parsed.Value.Container);

        var blob =
            container.GetBlobClient(
                parsed.Value.BlobName);

        if (!await blob.ExistsAsync(
            cancellationToken))
        {
            return null;
        }

        var response =
            await blob.DownloadStreamingAsync(
                cancellationToken:
                    cancellationToken);

        return response.Value.Content;
    }

    public async Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        var parsed = Parse(storageKey);

        if (parsed is null)
        {
            return;
        }

        var container =
            new BlobContainerClient(
                _options.AzureConnectionString,
                parsed.Value.Container);

        await container
            .GetBlobClient(
                parsed.Value.BlobName)
            .DeleteIfExistsAsync(
                cancellationToken:
                    cancellationToken);
    }

    private BlobContainerClient
        CreateContainerClient()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "Azure Blob Storage is not configured.");
        }

        return new BlobContainerClient(
            _options.AzureConnectionString,
            _options.AzureContainer
                .Trim()
                .ToLowerInvariant());
    }

    private static (
        string Container,
        string BlobName)?
        Parse(string storageKey)
    {
        if (!storageKey.StartsWith(
            Prefix,
            StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var value =
            storageKey[Prefix.Length..];

        var separator =
            value.IndexOf('/');

        if (separator <= 0 ||
            separator ==
            value.Length - 1)
        {
            throw new InvalidOperationException(
                "Invalid Azure Blob storage key.");
        }

        return (
            value[..separator],
            value[(separator + 1)..]);
    }

    private static string NormalizeFolder(
        string folder)
    {
        var segments = folder
            .Replace('\\', '/')
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(segment =>
                string.Concat(
                    segment
                        .ToLowerInvariant()
                        .Where(
                            character =>
                                char.IsLetterOrDigit(character) ||
                                character is '-' or '_')))
            .Where(segment =>
                !string.IsNullOrWhiteSpace(segment));

        return string.Join('/', segments);
    }
}
