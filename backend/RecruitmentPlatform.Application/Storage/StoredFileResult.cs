namespace RecruitmentPlatform.Application.Storage;

public sealed record StoredFileResult(
    string StorageKey,
    string StoredFileName,
    string Provider);
