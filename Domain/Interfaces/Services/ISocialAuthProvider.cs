using System.Security.Claims;

namespace Domain.Interfaces.Services
{
    public interface ISocialAuthProvider
    {
        Task<ClaimsPrincipal> ValidateToken(string provider, string token);
    }
}
