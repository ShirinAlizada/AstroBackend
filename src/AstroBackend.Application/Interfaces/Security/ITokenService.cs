using AstroBackend.Domain.Entities;

namespace AstroBackend.Application.Interfaces.Security
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
