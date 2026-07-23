using RecruitmentPlatform.Application.AI;
using RecruitmentPlatform.Application.Interfaces;

namespace RecruitmentPlatform.Infrastructure.AI;

public sealed class HybridCandidateFeedbackService
    : ICandidateFeedbackService
{
    private readonly OpenAiCandidateFeedbackClient
        _externalAi;
    private readonly RuleBasedCandidateFeedbackService
        _ruleBased;

    public HybridCandidateFeedbackService(
        OpenAiCandidateFeedbackClient externalAi,
        RuleBasedCandidateFeedbackService ruleBased)
    {
        _externalAi = externalAi;
        _ruleBased = ruleBased;
    }

    public async Task<CandidateFeedbackResult>
        GenerateAsync(
            CandidateFeedbackContext context,
            CancellationToken cancellationToken = default)
    {
        if (!_externalAi.IsConfigured)
        {
            return _ruleBased.Generate(
                context,
                "External AI is not configured; explainable rule-based feedback was used.");
        }

        try
        {
            return await _externalAi
                .GenerateAsync(
                    context,
                    cancellationToken);
        }
        catch (OperationCanceledException)
            when (!cancellationToken
                .IsCancellationRequested)
        {
            return _ruleBased.Generate(
                context,
                "External AI timed out; rule-based feedback was used.");
        }
        catch (Exception exception)
{
    return _ruleBased.Generate(
        context,
        $"External AI failed: {exception.Message}");
}
    }
}
