using Microsoft.Extensions.Hosting;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Application.Storage;

namespace RecruitmentPlatform.Infrastructure.Storage;

public sealed class LocalFileStorageService
{
    private const string Prefix = "local://";
    private readonly IHostEnvironment _environment;

    public LocalFileStorageService(
        IHostEnvironment environment)
    {
        _environment = environment;
    }

    public string ProviderName =>
        "Local App_Data Storage";

    public async Task<StoredFileResult> SaveAsync(
        Stream source,
        string originalFileName,
        string contentType,
        string folder,
        CancellationToken cancellationToken)
    {
        var extension =
            Path.GetExtension(originalFileName)
                .ToLowerInvariant();

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var safeFolder = NormalizeFolder(folder);

        var relativePath =
            Path.Combine(
                safeFolder,
                storedFileName);

        var fullDirectory =
            Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                safeFolder);

        Directory.CreateDirectory(fullDirectory);

        var fullPath =
            Path.Combine(
                fullDirectory,
                storedFileName);

        await using var destination =
            File.Create(fullPath);

        await source.CopyToAsync(
            destination,
            cancellationToken);

        var storageKey =
            Prefix +
            relativePath.Replace(
                Path.DirectorySeparatorChar,
                '/');

        return new StoredFileResult(
            storageKey,
            storedFileName,
            ProviderName);
    }

    public Task<Stream?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        var fullPath = ResolvePath(storageKey);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            FileOptions.Asynchronous |
            FileOptions.SequentialScan);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        var fullPath = ResolvePath(storageKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string storageKey)
    {
        // Supports legacy Phase 4-6 absolute paths.
        if (!storageKey.StartsWith(
            Prefix,
            StringComparison.OrdinalIgnoreCase))
        {
            return storageKey;
        }

        var relative =
            storageKey[Prefix.Length..]
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        var candidate =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data",
                    relative));

        var root =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data"));

        if (!candidate.StartsWith(
            root,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid local storage key.");
        }

        return candidate;
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
                    segment.Where(
                        character =>
                            char.IsLetterOrDigit(character) ||
                            character is '-' or '_')))
            .Where(segment =>
                !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        if (segments.Length == 0)
        {
            return "files";
        }

        return Path.Combine(segments);
    }
}
