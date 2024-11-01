using System.Security.Claims;
using ProjectMmApi.Models.Entities;

namespace ProjectMmApi.Interfaces
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        string CreateRefreshToken(User user);
        string CreateToken(Claim[] claims, TimeSpan exipry);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
