using Domain.Interfaces.Services;
using System.Security.Claims;

namespace Service
{
    public class SocialAuthProvider : ISocialAuthProvider
    {
        public Task<ClaimsPrincipal> ValidateToken(string provider, string token)
        {
            throw new NotImplementedException();
        }
    }
}
