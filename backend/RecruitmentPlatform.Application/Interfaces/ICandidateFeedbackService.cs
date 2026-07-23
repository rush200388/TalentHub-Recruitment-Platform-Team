using RecruitmentPlatform.Application.AI;

namespace RecruitmentPlatform.Application.Interfaces;

public interface ICandidateFeedbackService
{
    Task<CandidateFeedbackResult> GenerateAsync(
        CandidateFeedbackContext context,
        CancellationToken cancellationToken = default);
}
