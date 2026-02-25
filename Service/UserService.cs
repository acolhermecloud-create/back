using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Repository.System;
using Domain.Interfaces.Services;
using Domain.Objects;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;
using System.Net.Sockets;

namespace Service
{
    public class UserService(IUserRepository userRepository,
        IOngRepository ongRepository,
        IUserGroupAccessRepository userGroupAccessRepository,
        IGroupAccessRepository groupAccessRepository,
        IUtilityService utilityService,
        IConfiguration configuration,
        IS3Service s3Service,
        IAddressRepository addressRepository,
        IUserPointsRepository userPointsRepository,
        ICodeChallengeRepository codeChallengeRepository,
        IEmailService emailService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IOngRepository _ongRepository = ongRepository;
        private readonly IAddressRepository _addressRepository = addressRepository;
        private readonly IGroupAccessRepository _groupAccessRepository = groupAccessRepository;
        private readonly IUserGroupAccessRepository _userGroupAccessRepository = userGroupAccessRepository;
        private readonly IUtilityService _utilityService = utilityService; // Interface para hash de senhas
        private readonly ISocialAuthProvider _socialAuthProvider; // Interface para autenticação social
        private readonly IUserPointsRepository _userPointsRepository = userPointsRepository; // Interface para autenticação social
        private readonly ICodeChallengeRepository _codeChallengeRepository = codeChallengeRepository; // Interface para autenticação social

        private readonly IConfiguration _configuration = configuration;
        private readonly IEmailService _emailService = emailService;
        private readonly IS3Service _s3Service = s3Service;

        public async Task<string> Create(string email, string name, string password, string documetnId,
            UserType type = UserType.Common, string? phone = null)
        {
            // Verifica se o usuário já existe
            var existingUser = await _userRepository.GetByEmail(email);
            
            if (existingUser != null)
                throw new Exception("Email já cadastrado");

            if (!string.IsNullOrEmpty(documetnId))
            {
                var existingUserByCpf = await _userRepository.GetByDocumentId(documetnId);
                if (existingUserByCpf != null)
                    throw new Exception("CPF/CNPJ já cadastrado");
            }

            // Cria e salva o novo usuário
            var hashedPassword = _utilityService.CryptSHA256(password);
            var newUser = new User(name, email, hashedPassword, documetnId, AuthProvider.None, type, null, phone);
            await _userRepository.Add(newUser);

            string nameGroupAccess = string.Empty;

            if (type == UserType.Common || type == UserType.Ong)
                nameGroupAccess = "User";
            else
                nameGroupAccess = "Admin";

            var groupAcess = await _groupAccessRepository.GetByName(nameGroupAccess);

            var userGroupAcess = new UserGroupAccess(newUser.Id, groupAcess.Id);
            await _userGroupAccessRepository.Add(userGroupAcess);

            return newUser.Id.ToString();
        }

        public async Task<string> GetOrCreate(string email, string name, string password, string documetnId,
            UserType type = UserType.Common, string? phone = null)
        {
            // Verifica se o usuário já existe
            var existingUser = await _userRepository.GetByEmail(email);

            if (existingUser != null)
            {
                if (type == UserType.Common)
                    return existingUser.Id.ToString();
                else
                    throw new Exception("Email já cadastrado");
            }

            if (!string.IsNullOrEmpty(documetnId))
            {
                var existingUserByCpf = await _userRepository.GetByDocumentId(documetnId);
                if (existingUserByCpf != null)
                    throw new Exception("CPF/CNPJ já cadastrado");
            }

            // Cria e salva o novo usuário
            var hashedPassword = _utilityService.CryptSHA256(password);
            var newUser = new User(name, email, hashedPassword, documetnId, AuthProvider.None, type, null, phone);
            await _userRepository.Add(newUser);

            string nameGroupAccess = string.Empty;

            if (type == UserType.Common || type == UserType.Ong)
                nameGroupAccess = "User";
            else
                nameGroupAccess = "Admin";

            var groupAcess = await _groupAccessRepository.GetByName(nameGroupAccess);

            var userGroupAcess = new UserGroupAccess(newUser.Id, groupAcess.Id);
            await _userGroupAccessRepository.Add(userGroupAcess);

            return newUser.Id.ToString();
        }

        public async Task<User> Update(Guid id, string email, string name, string phone, string? secret)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Os campos email, nome e telefone são obrigatórios.");

            var user = await _userRepository.GetById(id) ?? throw new Exception("Usuário não encontrado.");

            var existingUser = await _userRepository.GetByEmail(email);
            if (existingUser != null && existingUser.Id != id)
                throw new InvalidOperationException("Email já está sendo usado.");

            // Atualiza os campos do usuário
            user.Email = email;
            user.Name = name;
            user.Phone = phone;
            user.TwoFactorSecret = secret;

            await _userRepository.Update(user);

            return user;
        }

        public async Task UpdatePassword(Guid id, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetById(id) ?? throw new Exception("Usuário não encontrado.");

            if (_utilityService.CryptSHA256(oldPassword) != user.Password)
                throw new Exception("Senha antiga incorreta");

            user.Password = _utilityService.CryptSHA256(newPassword);
            await _userRepository.Update(user);
        }

        public async Task RequestChallege(string userMail)
        {
            var userByMail = await _userRepository.GetByEmail(userMail);
            if (userByMail == null) throw new Exception("Usuário não encontrado");

            var code = _utilityService.GenerateCode(6);

            CodeChallenge codeChallenge = new(userMail, code, DateTime.Now.AddMinutes(10));
            await _codeChallengeRepository.Add(codeChallenge);

            string subject = "Solicitação - Alterar Senha";

            string body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                        <h2 style='color: #333;'>Alteração de Senha</h2>
                        <p>Você solicitou a alteração de senha. Utilize o código abaixo para continuar:</p>
                        <h3 style='background: #f4f4f4; display: inline-block; padding: 10px 20px; border-radius: 5px;'>
                            {code}
                        </h3>
                        <p>Se você não fez essa solicitação, ignore este e-mail.</p>
                        <p style='color: #888; font-size: 12px;'>Este e-mail foi gerado automaticamente. Não responda.</p>
                    </body>
                </html>
            ";

            await SendMailNotification(subject, body, userMail);
        }

        public async Task UpdatePasswordWithChallenge(string userEmail, string code, string newPassword)
        {
            var challenge = await _codeChallengeRepository.GetByReferenceAndCodeValid(userEmail, code);

            if (challenge == null) throw new Exception("Código inválido");

            var user = await _userRepository.GetByEmail(userEmail) ?? throw new Exception("Usuário não encontrado.");

            user.Password = _utilityService.CryptSHA256(newPassword);
            await _userRepository.Update(user);
        }

        public async Task Delete(Guid id)
        {
            var user = await _userRepository.GetById(id) ?? throw new Exception("Usuário não encontrado.");

            await _userGroupAccessRepository.DeleteByUserId(id); // Remove associações do usuário
            await _userRepository.Delete(id); // Remove o usuário
        }

        public async Task<User> GetByEmail(string email)
        {
            var user = await _userRepository.GetByEmail(email);

            if (user == null)
                return null;

            if (user.AddressId != null)
                user.Address = await _addressRepository.GetById(Guid.Parse($"{user.AddressId}"));

            return user;
        }

        public async Task<User> GetByDocument(string document)
        {
            var user = await _userRepository.GetByDocumentId(document);

            if (user == null)
                return null;

            if (user.AddressId != null)
                user.Address = await _addressRepository.GetById(Guid.Parse($"{user.AddressId}"));

            return user;
        }

        public async Task<PagedResult<User>> ListAll(string? name, UserType userType, int page, int pageSize)
        {
            var users = await _userRepository.GetWithFilter(name, userType, page, pageSize);

            // Remove a senha antes de retornar
            foreach (var user in users.Items)
                user.Password = null; // Remove a senha para não expor os dados sensíveis

            return users;
        }

        public async Task<List<User>> SearchByName(string name)
        {
            return await _userRepository.SearchByName(name);
        }

        public async Task<string> UpdateAvatar(Guid id, Stream stream, string extension)
        {
            var user = await _userRepository.GetById(id);

            if (user == null) throw new Exception("Usuário inexistente");

            var filenamekey = await _s3Service.SendStreamFileToS3(stream, extension);

            if(!string.IsNullOrEmpty(user.AvatarKey))
                await _s3Service.DeleteFileByFileNameKey(user.AvatarKey);
            
            user.AvatarKey = filenamekey;
            await _userRepository.Update(user);

            return await _s3Service.GetFileUrlByFileNameKey(filenamekey);
        }

        public async Task RemoveAvatar(Guid userId)
        {
            var user = await _userRepository.GetById(userId);

            if (user == null) throw new Exception("Usuário inexistente");

            if (!string.IsNullOrEmpty(user.AvatarKey))
                await _s3Service.DeleteFileByFileNameKey(user.AvatarKey);

            user.AvatarKey = string.Empty;
            await _userRepository.Update(user);
        }

        public async Task UpdateAddress(Guid userId, string street, string city, string state, string zipCode, string country)
        {
            var user = await _userRepository.GetById(userId);

            if(user.AddressId != null)
            {
                var addressreposiotry = await _addressRepository.GetById(Guid.Parse($"{user.AddressId}"));
                
                addressreposiotry.Street = street;
                addressreposiotry.City = city;
                addressreposiotry.State = state;
                addressreposiotry.ZipCode = zipCode;
                addressreposiotry.Country = country;

                await _addressRepository.Update(addressreposiotry);
            }
            else
            {
                var address = new Address(street, city, state, zipCode, country);
                await _addressRepository.Add(address);

                user.AddressId = address.Id;
                await _userRepository.Update(user);
            }
        }

        public async Task<User> GetById(Guid id) => await _userRepository.GetById(id);

        public async Task CreateOng(Guid ownerId, Guid categoryId, 
            string name, string description, 
            string about, string site,
            string mail, string phone,
            string instagram,
            string youtube,
            Dictionary<Stream, string> banner,
            string street, string city, 
            string state, string zipCode, string country)
        {
            var filenamekey = await _s3Service.SendStreamFileToS3(banner.Keys.First(), banner.Values.First());

            var address = new Address(street, city, state, zipCode, country);
            var ong = new Ong(
                name, description, about, site, mail, phone, instagram, youtube, filenamekey, categoryId, 
                address.Id, ownerId);

            await _addressRepository.Add(address);
            await _ongRepository.Add(ong);

        }

        public async Task UpdateOng(Guid ongId, Guid categoryId, 
            string name, string description, string about, 
            string site, string mail, string phone, string instagram,
            string youtube,
            string street, string city, 
            string state, string zipCode, string country)
        {
            var ong = await _ongRepository.GetById(ongId) ?? throw new Exception("Ong não existe");

            var address = await _addressRepository.GetById(ong.AddressId);
            if(address != null)
            {
                address.Street = street;
                address.City = city;
                address.State = state;
                address.ZipCode = zipCode;
                address.Country = country;
                await _addressRepository.Update(address);
            }

            ong.Name = name;
            ong.Description = description;
            ong.About = about;
            ong.Site = site;
            ong.Mail = mail;
            ong.Phone = phone;
            ong.Instagram = instagram;
            ong.Youtube = youtube;
            ong.CategoryId = categoryId;
            
            await _ongRepository.Update(ong);
        }

        public async Task UpdateOngBanner(Guid ongId, Stream file, string extension)
        {
            var ong = await _ongRepository.GetById(ongId) ?? throw new Exception("Ong não existe");

            if (!string.IsNullOrEmpty(ong.BannerKey))
                await _s3Service.DeleteFileByFileNameKey(ong.BannerKey);

            var filenamekey = await _s3Service.SendStreamFileToS3(file, extension);

            ong.BannerKey = filenamekey;
            await _ongRepository.Update(ong);
        }

        public async Task<List<Ong>> GetOngs(int size, int pageId)
        {
            var ongs = await _ongRepository.GetAll(size, pageId);

            foreach (var ong in ongs)
            {
                var user = await _userRepository.GetById(ong.OwnerId);
                user.Password = string.Empty;

                ong.User = user;

                var address = await _addressRepository.GetById(ong.AddressId);
                ong.Address = address;
            }

            return ongs;
        }

        public async Task IncrementPointsToUser(Guid userId, int amount)
        {
            var points = await _userPointsRepository.GetByUserId(userId);

            if(points == null)
            {
                var addedPoints = new AddedPoints
                {
                    Amount = amount,
                    CreatedAt = DateTime.Now
                };

                var userPoints = new UserPoints(userId, amount, [addedPoints]);
                await _userPointsRepository.Add(userPoints);
            }
            else
            {
                points.AddedPoints.Add(new AddedPoints() { Amount = amount, CreatedAt = DateTime.Now });
                points.CurrentPoints += amount;

                await _userPointsRepository.Update(points);
            }
        }

        public async Task<List<User>> GetUsersWhoHavePointsGreaterThan(int amount, int page, int pageSize)
        {
            var points = await _userPointsRepository.GetMoreThan(amount, page, pageSize);

            var users = new List<User>();
            foreach (var point in points)
            {
                var user = await _userRepository.GetById(point.UserId);
                if (user == null)
                    continue;

                user.UserPoints = point;
                users.Add(user);
            }
            return users;
        }

        protected async Task SendMailNotification(string subject, string body, string receptor)
        {
            string[] receivers = [receptor];
            string title = subject;
            string businessAddresses = _configuration["Mail:SenderEmail"];
            string businessName = _configuration["Mail:SenderName"];

            await _emailService.Send(receivers, title, body, businessAddresses,
                businessName, null);
        }

        public async Task ChangeStatus(Guid id, UserStatus newStatus)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return;

            user.Status = newStatus;
            await _userRepository.Update(user);
        }
    }
}