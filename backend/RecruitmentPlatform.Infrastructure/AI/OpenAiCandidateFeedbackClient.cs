using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RecruitmentPlatform.Application.AI;

namespace RecruitmentPlatform.Infrastructure.AI;

public sealed class OpenAiCandidateFeedbackClient
{
    private static readonly JsonSerializerOptions
        JsonOptions =
            new(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive =
                    true
            };

    private static readonly HashSet<string>
        AllowedRecommendations =
            new(
                StringComparer.OrdinalIgnoreCase)
            {
                "Strong Hire",
                "Hire",
                "Consider",
                "Reject",
                "Strong Reject"
            };

    private readonly HttpClient _httpClient;
    private readonly AiProviderOptions _options;

    public OpenAiCandidateFeedbackClient(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public bool IsConfigured =>
        _options.Enabled &&
        !string.IsNullOrWhiteSpace(
            _options.ApiKey) &&
        !string.IsNullOrWhiteSpace(
            _options.Endpoint) &&
        !string.IsNullOrWhiteSpace(
            _options.Model);

    public async Task<CandidateFeedbackResult>
        GenerateAsync(
            CandidateFeedbackContext context,
            CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "External AI is not configured.");
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                _options.Endpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);

        var safeContext =
            JsonSerializer.Serialize(
                context,
                JsonOptions);

        request.Content =
            JsonContent.Create(
                new
                {
                    model = _options.Model,
                    instructions =
                        """
                        You generate fair, job-related recruitment feedback.
                        Use only the supplied skills, experience, match score,
                        interview score, and evaluation score. Do not infer
                        protected attributes. Do not make the final hiring
                        decision. Return only one JSON object with exactly:
                        summary (string), strengths (string array),
                        risks (string array), recommendation (one of
                        "Strong Hire", "Hire", "Consider", "Reject",
                        "Strong Reject"), suggestedFeedback (string).
                        """,
                    input =
                        $"Generate recruitment feedback from this de-identified evidence:\n{safeContext}"
                });

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var body =
            await response.Content
                .ReadAsStringAsync(
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"External AI returned HTTP {(int)response.StatusCode}.");
        }

        var text =
            ExtractOutputText(body);

        var content =
            DeserializeFeedback(text);

        if (string.IsNullOrWhiteSpace(
            content.Summary) ||
            string.IsNullOrWhiteSpace(
                content.SuggestedFeedback))
        {
            throw new InvalidOperationException(
                "External AI returned incomplete feedback.");
        }

        var recommendation =
            AllowedRecommendations.Contains(
                content.Recommendation ?? string.Empty)
                ? AllowedRecommendations.Single(
                    item =>
                        string.Equals(
                            item,
                            content.Recommendation,
                            StringComparison.OrdinalIgnoreCase))
                : "Consider";

        return new CandidateFeedbackResult(
            $"OpenAI/{_options.Model}",
            true,
            content.Summary.Trim(),
            NormalizeList(
                content.Strengths),
            NormalizeList(
                content.Risks),
            recommendation,
            content.SuggestedFeedback.Trim(),
            DateTime.UtcNow,
            null);
    }

    private static string ExtractOutputText(
        string responseJson)
    {
        using var document =
            JsonDocument.Parse(responseJson);

        if (!document.RootElement
            .TryGetProperty(
                "output",
                out var output) ||
            output.ValueKind !=
                JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "External AI response did not contain output.");
        }

        var textParts = new List<string>();

        foreach (var item in
            output.EnumerateArray())
        {
            if (!item.TryGetProperty(
                "content",
                out var content) ||
                content.ValueKind !=
                    JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in
                content.EnumerateArray())
            {
                if (contentItem.TryGetProperty(
                    "type",
                    out var type) &&
                    type.GetString() ==
                        "output_text" &&
                    contentItem.TryGetProperty(
                        "text",
                        out var text))
                {
                    var value =
                        text.GetString();

                    if (!string.IsNullOrWhiteSpace(
                        value))
                    {
                        textParts.Add(value);
                    }
                }
            }
        }

        if (textParts.Count == 0)
        {
            throw new InvalidOperationException(
                "External AI response contained no text output.");
        }

        return string.Join(
            Environment.NewLine,
            textParts);
    }

    private static ExternalFeedbackContent
        DeserializeFeedback(string value)
    {
        var cleaned = value.Trim();

        if (cleaned.StartsWith(
            "```",
            StringComparison.Ordinal))
        {
            var firstNewLine =
                cleaned.IndexOf('\n');

            var lastFence =
                cleaned.LastIndexOf(
                    "```",
                    StringComparison.Ordinal);

            if (firstNewLine >= 0 &&
                lastFence > firstNewLine)
            {
                cleaned =
                    cleaned[
                        (firstNewLine + 1)..
                        lastFence]
                    .Trim();
            }
        }

        var firstBrace =
            cleaned.IndexOf('{');
        var lastBrace =
            cleaned.LastIndexOf('}');

        if (firstBrace >= 0 &&
            lastBrace > firstBrace)
        {
            cleaned =
                cleaned[
                    firstBrace..
                    (lastBrace + 1)];
        }

        return JsonSerializer
            .Deserialize<
                ExternalFeedbackContent>(
                cleaned,
                JsonOptions)
            ?? throw new InvalidOperationException(
                "External AI feedback JSON could not be parsed.");
    }

    private static IReadOnlyCollection<string>
        NormalizeList(
            IEnumerable<string>? values)
    {
        return (values ?? [])
            .Select(value => value.Trim())
            .Where(value =>
                !string.IsNullOrWhiteSpace(
                    value))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
    }

    private sealed class ExternalFeedbackContent
    {
        public string Summary { get; set; } =
            string.Empty;

        public List<string> Strengths
            { get; set; } = [];

        public List<string> Risks
            { get; set; } = [];

        public string Recommendation
            { get; set; } = "Consider";

        public string SuggestedFeedback
            { get; set; } = string.Empty;
    }
}
