using RecruitmentPlatform.Application.Matching;
using RecruitmentPlatform.Domain.Entities;

namespace RecruitmentPlatform.Application.Interfaces;

public interface IJobMatchingService
{
    JobMatchResult Calculate(
        CandidateProfile candidate,
        Job job);
}
