namespace Domain.Interfaces.Repository
{
    public interface IUserGroupAccessRepository
    {
        // Adiciona uma nova associação entre usuário e grupo de acesso
        Task Add(UserGroupAccess userGroupAccess);

        // Remove uma associação pelo ID
        Task Delete(Guid id);

        Task DeleteByUserId(Guid id);

        // Obtém uma associação por ID
        Task<UserGroupAccess> GetById(Guid id);

        // Obtém todas as associações de um usuário específico
        Task<List<UserGroupAccess>> GetByUserId(Guid userId);

        // Obtém todas as associações de um grupo de acesso específico
        Task<List<UserGroupAccess>> GetByGroupAccessId(Guid groupAccessId);

        // Verifica se uma associação entre usuário e grupo de acesso já existe
        Task<bool> Exists(Guid userId, Guid groupAccessId);
    }
}