namespace RecruitmentPlatform.Infrastructure.AI;

public sealed class AiProviderOptions
{
    public const string SectionName = "AiProvider";

    public bool Enabled { get; set; }
    public string Endpoint { get; set; } =
        "https://api.openai.com/v1/responses";
    public string Model { get; set; } = "gpt-5.6";
    public string ApiKey { get; set; } = string.Empty;
}
