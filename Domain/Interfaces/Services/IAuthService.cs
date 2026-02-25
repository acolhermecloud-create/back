using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthDataDto> AuthenticateUserWithPassword(string email, string password, UserType type = UserType.Common);

        Task<AuthDataDto> FinalizeAuthentication(string email, string otp);

        Task<User> AuthenticateWithSocialMedia(string provider, string token);

        Task RecoverPassword(string email);

        bool ValidateToken(string token);

        Task<string> CreateUrlTo2FA(Guid userId);

        Task<AuthDataDto> Save2Fa(string email, string code);

        Task<AuthDataDto> Delete2Fa(Guid userId);
    }
}
