using Domain;
using Domain.Interfaces.Services;
using Domain.System;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Util
{
    public class UtilityService : IUtilityService
    {
        const string chars_string = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly IConfiguration _configuration;

        public UtilityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                string mimeType = GetMimeType(imageUrl);
                string base64String = Convert.ToBase64String(imageBytes);
                return $"data:{mimeType};base64,{base64String}";
            }
        }

        public Stream ConvertBase64ToStream(string base64Input)
        {
            if (string.IsNullOrWhiteSpace(base64Input))
                throw new ArgumentException("Base64 input is empty.");

            // Se tiver prefixo, remove
            if (base64Input.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                var parts = base64Input.Split(',');
                if (parts.Length != 2)
                    throw new FormatException("Invalid base64 image format.");
                base64Input = parts[1];
            }

            // Remove quebras de linha e espaços
            base64Input = base64Input.Replace("\r", "").Replace("\n", "").Replace(" ", "");

            // Corrige tamanho inválido (múltiplo de 4)
            int mod4 = base64Input.Length % 4;
            if (mod4 > 0)
                base64Input += new string('=', 4 - mod4);

            byte[] imageBytes = Convert.FromBase64String(base64Input);
            return new MemoryStream(imageBytes);
        }


        private static string GetMimeType(string imageUrl)
        {
            string extension = imageUrl.Substring(imageUrl.LastIndexOf('.') + 1).ToLower();
            return extension switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        public string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("O título não pode ser nulo ou vazio.", nameof(title));

            // Remove acentos
            string normalizedTitle = title.Normalize(System.Text.NormalizationForm.FormD);
            var withoutAccents = new string(normalizedTitle
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) !=
                            System.Globalization.UnicodeCategory.NonSpacingMark)
                .ToArray());

            // Remove caracteres especiais
            string cleanedTitle = new string(withoutAccents
                .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-')
                .ToArray());

            // Substituir espaços por hífens e converter para minúsculas
            string slug = cleanedTitle.Trim().ToLower().Replace(" ", "-");

            // Remover múltiplos hífens consecutivos
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            return slug;
        }


        public string CryptSHA256(string str)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(str));

                foreach (byte b in result)
                {
                    Sb.Append(b.ToString("x2"));
                }
            }

            return Sb.ToString();
        }

        public string CryptWithSecretKey(string text)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_configuration["Crypt:Secret"]);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new((Stream)cryptoStream))
            {
                streamWriter.Write(text);
            }
            array = memoryStream.ToArray();

            return Convert.ToBase64String(array);
        }

        public string DecryptBySecretKey(string textocriptografado)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(textocriptografado);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_configuration["Crypt:Secret"]);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader((Stream)cryptoStream);
            return streamReader.ReadToEnd();
        }

        public bool EmailValidator(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateToken(User user, List<GroupAccess> groupAccess)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Crypt:Secret"]);

            var claims = new List<Claim>
            {
                new(ClaimTypes.UserData, JsonConvert.SerializeObject(user))
            };

            foreach (var permission in groupAccess)
            {
                claims.Add(new Claim(ClaimTypes.Name, permission.Name.ToLower()));
                claims.Add(new Claim("Store", permission.Name.ToLower()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User GetUserByToken(string token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var validTo = jwtToken.ValidTo;
            if (validTo < DateTime.UtcNow)
                throw new Exception("Token inválido");

            var userDataClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;
            var user = JsonConvert.DeserializeObject<User>(userDataClaim);

            return user;
        }

        public ClaimsPrincipal GetClaimsFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var claims = jwtToken.Claims;
            var identity = new ClaimsIdentity(claims);

            return new ClaimsPrincipal(identity);
        }

        public bool ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Crypt:Secret"]);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                //var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value);

                if (DateTime.UtcNow > jwtToken.ValidTo.ToUniversalTime())
                    return false;

                // return account id from JWT token if validation successful
                return true;
            }
            catch
            {
                // return null if validation fails
                return false;
            }
        }

        public string GenerateCode(int size)
        {
            Random _random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, size)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public decimal ConvertCentsToDecimal(long cents)
        {
            return cents / 100m;
        }

        public long ConvertFloatToLong(float value)
        {
            return (long)(value * 100); // Multiplica por 100 e converte para long
        }
    }
}
