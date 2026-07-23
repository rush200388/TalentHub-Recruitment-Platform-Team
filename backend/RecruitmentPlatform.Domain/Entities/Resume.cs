using RecruitmentPlatform.Domain.Common;

namespace RecruitmentPlatform.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string? ParsedText { get; set; }
    public string AnalysisStatus { get; set; } = "NotAnalyzed";
    public string? AnalysisStrategy { get; set; }
    public string? AnalysisJson { get; set; }
    public DateTime? AnalyzedAtUtc { get; set; }

    public bool IsPrimary { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
