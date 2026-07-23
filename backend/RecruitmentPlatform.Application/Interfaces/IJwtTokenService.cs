using RecruitmentPlatform.Application.Security;

namespace RecruitmentPlatform.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(
        string userId,
        string email,
        string firstName,
        string lastName,
        IEnumerable<string> roles);
}
