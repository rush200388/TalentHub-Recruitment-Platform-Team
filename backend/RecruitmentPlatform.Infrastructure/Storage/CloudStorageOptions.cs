namespace RecruitmentPlatform.Infrastructure.Storage;

public sealed class CloudStorageOptions
{
    public const string SectionName = "CloudStorage";

    public string Provider { get; set; } = "Local";
    public string AzureConnectionString { get; set; } = string.Empty;
    public string AzureContainer { get; set; } = "recruitment-files";
}
