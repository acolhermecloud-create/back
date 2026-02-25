using Domain.Objects;

namespace Domain.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task Add(User user);

        Task Update(User user);

        Task Delete(Guid id);

        Task<User> GetById(Guid id);

        Task<User> GetByEmail(string email);

        Task<User> GetByDocumentId(string documentId);

        Task<List<User>> SearchByName(string name);

        Task<PagedResult<User>> GetWithFilter(string? name, UserType userType, int page, int pageSize);

        Task<bool> EmailExists(string email);

        Task<long> TotalUsers(UserType userType);
    }
}
