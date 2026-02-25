using Domain.System;
using System.Security.Claims;

namespace Domain.Interfaces.Services
{
    public interface IUtilityService
    {
        string GenerateToken(User user, List<GroupAccess> groupAccess);

        string GenerateSlug(string title);

        string GenerateCode(int size);

        User GetUserByToken(string token);

        bool ValidateJwtToken(string token);

        ClaimsPrincipal GetClaimsFromToken(string token);

        string CryptSHA256(string str);

        string CryptWithSecretKey(string text);

        string DecryptBySecretKey(string textCrypt);

        bool EmailValidator(string email);

        Task<string> DownloadImageAsBase64(string imageUrl);

        Stream ConvertBase64ToStream(string base64String);

        decimal ConvertCentsToDecimal(long cents);

        long ConvertFloatToLong(float value);
    }
}
