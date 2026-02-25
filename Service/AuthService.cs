using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.Objects;
using Domain.System;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OtpNet;
using QRCoder;
using System.Security.Claims;
using Util;

namespace Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IGroupAccessRepository _groupAccessRepository;
        private readonly IUserGroupAccessRepository _userGroupAccessRepository;
        private readonly IUtilityService _utilityService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly ISocialAuthProvider _socialAuthProvider; // Interface para autenticação social
        private readonly ICacheService _cacheService;

        private readonly IS3Service _s3Service;

        string TEMPLATE_RESET_PASSWORD = $@"Templates{Path.DirectorySeparatorChar}RESETPASSWORD.html";

        public AuthService(
            IUserService userService,
            IUtilityService utilityService,
            ISocialAuthProvider socialAuthProvider,
            IMailService mailService,
            IConfiguration configuration,
            IGroupAccessRepository groupAccessRepository,
            IUserGroupAccessRepository userGroupAccessRepository,
            IS3Service s3Service,
            ICacheService cacheService)
        {
            _userService = userService;
            _groupAccessRepository = groupAccessRepository;
            _userGroupAccessRepository = userGroupAccessRepository;
            _utilityService = utilityService;
            _mailService = mailService;
            _configuration = configuration;
            _s3Service = s3Service;
            _cacheService = cacheService;
        }

        public async Task<User> AuthenticateWithSocialMedia(string provider, string token)
        {
            var claims = await _socialAuthProvider.ValidateToken(provider, token);
            var email = (claims.FindFirst(ClaimTypes.Email)?.Value) ?? throw new Exception("Token inválido.");

            // Verifica se o usuário já existe, caso contrário, cria um novo
            var user = await _userService.GetByEmail(email);
            if (user == null)
            {
                await _userService.Create(email, claims.FindFirst(ClaimTypes.Name)?.Value, "", "", UserType.Common);
            }
            return user;
        }

        public async Task<AuthDataDto> AuthenticateUserWithPassword(string email, string password, UserType type)
        {
            var user = await _userService.GetByEmail(email);

            if (user == null || user.Password != _utilityService.CryptSHA256(password))
                throw new Exception("Email ou senha inválidos.");

            if (user.Type != type) throw new Exception("Usuário não encontrado");

            if (user.Status == UserStatus.Inactive) throw new Exception("Usuário bloqueado");

            if (!string.IsNullOrEmpty(user.TwoFactorSecret)) {
                return new AuthDataDto
                {
                    TwoFactorActive = true
                };
            }

            return await GenerateAuthData(user);
        }

        public async Task<AuthDataDto> FinalizeAuthentication(string email, string otp)
        {
            var user = await _userService.GetByEmail(email);

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if(totp.VerifyTotp(otp, out _))
            {
                return await GenerateAuthData(user);
            }
            else
                throw new Exception("Código inválido");
        }

        public async Task RecoverPassword(string email)
        {
            var user = await _userService.GetByEmail(email);

            if (user == null || user.Provider != AuthProvider.None)
                throw new Exception("Usuário não encontrado ou não é um usuário local.");

            string systemName = _configuration["System:Name"];
            string resetToken = await AuthenticateWithEmail(user); // Gera token
            var frontEndUrl = _configuration["System:FrontendUrl"];
            var pathRecoveryPass = _configuration["System:PathRecoveryPass"];

            string linkRedefinition = $"{frontEndUrl}{pathRecoveryPass}?token={resetToken}";
            string subject = $"[{systemName}] - Solicitação de Redefinição de senha";

            HtmlDocument htmlDoc = new();
            htmlDoc.Load(TEMPLATE_RESET_PASSWORD);

            var filledTemplate = Functions.ReplaceVariable(htmlDoc.Text, "linkRedefinition", linkRedefinition);

            await _mailService.SendEmailAsync(email, subject, filledTemplate);
        }

        public bool ValidateToken(string token) => _utilityService.ValidateJwtToken(token);

        protected async Task<string> AuthenticateWithEmail(User user)
        {
            var userPermissions = await _userGroupAccessRepository.GetByUserId(user.Id);
            List<GroupAccess> permissions = [];

            foreach (var permission in userPermissions)
            {
                var groupaccessrepostitory = await _groupAccessRepository.GetById(permission.Id);
                if (groupaccessrepostitory != null)
                    permissions.Add(groupaccessrepostitory);
            }

            string token = _utilityService.GenerateToken(user, permissions);

            return token;
        }

        public async Task<string> CreateUrlTo2FA(Guid userId)
        {
            var user = await _userService.GetById(userId);

            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key);

            Otp2faCache temp = new()
            {
                Email = user.Email,
                Secret = secret
            };
            await _cacheService.Set(user.Email, JsonConvert.SerializeObject(temp), 8);

            string issuer = _configuration["System:Name"];
            string account = user.Email;
            string url = $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}";

            // Gerar QR Code
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            // Converter para Base64
            string base64Image = Convert.ToBase64String(qrCodeBytes);

            return $"data:image/png;base64,{base64Image}";
        }

        public async Task<AuthDataDto> Save2Fa(string email, string code)
        {
            var cache = await _cacheService.Get(email);
            if (cache == null) throw new Exception("Gere um QR Code novamente");

            var twofaTemp = JsonConvert.DeserializeObject<Otp2faCache>(cache);

            var totp = new Totp(Base32Encoding.ToBytes(twofaTemp.Secret));
            if (totp.VerifyTotp(code, out _))
            {
                var user = await _userService.GetByEmail(email);
                user.TwoFactorSecret = twofaTemp.Secret;
                await _userService.Update(user.Id, user.Email, user.Name, user.Phone ?? "", twofaTemp.Secret);

                return await GenerateAuthData(user);
            }
            else
                throw new Exception("Código inválido");
        }

        protected async Task<AuthDataDto> GenerateAuthData(User user)
        {
            var userPermissions = await _userGroupAccessRepository.GetByUserId(user.Id);
            List<GroupAccess> permissions = [];

            foreach (var permission in userPermissions)
            {
                var groupaccessrepostitory = await _groupAccessRepository.GetById(permission.GroupAccessId);
                if (groupaccessrepostitory != null)
                    permissions.Add(groupaccessrepostitory);
            }

            string token = _utilityService.GenerateToken(user, permissions);

            if (!string.IsNullOrEmpty(user.AvatarKey))
                user.AvatarUrl = await _s3Service.GetFileUrlByFileNameKey(user.AvatarKey);

            return new AuthDataDto
            {
                TwoFactorActive = !string.IsNullOrEmpty(user.TwoFactorSecret),
                Token = token,
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                DocumentId = user.DocumentId,
                Avatar = user.AvatarUrl,
                Address = user.Address,
            };
        }

        public async Task<AuthDataDto> Delete2Fa(Guid userId)
        {
            var user = await _userService.GetById(userId);
            await _userService.Update(user.Id, user.Email, user.Name, user.Phone ?? "", null);

            user.TwoFactorSecret = null; // Limpa o estado atual

            return await GenerateAuthData(user);
        }
    }
}
