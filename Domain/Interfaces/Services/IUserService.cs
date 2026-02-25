using Domain.Objects;

namespace Domain.Interfaces.Services
{
    public interface IUserService
    {
        // Cria um novo usuário
        Task<string> Create(string email, string name, string password, string documentId, UserType type, string? phone = null);

        Task<string> GetOrCreate(string email, string name, string password, string documentId, UserType type, string? phone = null);

        // Edita um usuário existente
        Task<User> Update(Guid userId, string email, string name, string phone, string? secret = null);

        Task UpdatePassword(Guid userId, string oldPassword, string newPassword);

        Task RequestChallege(string userMail);

        Task UpdatePasswordWithChallenge(string userEmail, string code, string newPassword);

        Task<string> UpdateAvatar(Guid userId, Stream stream, string extension);

        Task RemoveAvatar(Guid userId);

        Task UpdateAddress(Guid userId, string street, string city, string state, string zipCode, string country);

        // Remove um usuário
        Task Delete(Guid userId);

        Task<User> GetByEmail(string email);

        Task<User> GetByDocument(string document);

        Task<User> GetById(Guid id);

        Task ChangeStatus(Guid id, UserStatus newStatus);

        // Lista todos os usuários
        Task<PagedResult<User>> ListAll(string? name, UserType userType, int page, int pageSize);

        Task<List<User>> SearchByName(string name);

        Task CreateOng(Guid ownerId,
            Guid categoryId,
            string name,
            string description,
            string about,
            string site,
            string mail,
            string phone,
            string instagram,
            string youtube,
            Dictionary<Stream, string> banner,
            string street,
            string city,
            string state,
            string zipCode,
            string country);

        Task UpdateOng(Guid ongId,
            Guid categoryId,
            string name,
            string description,
            string about,
            string site,
            string mail,
            string phone,
            string instagram,
            string youtube,
            string street,
            string city,
            string state,
            string zipCode,
            string country);

        Task UpdateOngBanner(Guid ongId, Stream file, string extension);

        Task<List<Ong>> GetOngs(int size, int pageId);

        Task IncrementPointsToUser(Guid userId, int amount);

        Task<List<User>> GetUsersWhoHavePointsGreaterThan(int amount, int page, int pageSize);
    }
}
