using Domain.System;

namespace Domain.Interfaces.Repository.System
{
    public interface IGroupAccessRepository
    {
        // Adiciona um novo grupo de acesso
        Task Add(GroupAccess groupAccess);

        // Obtém um grupo de acesso por ID
        Task<GroupAccess> GetById(Guid id);

        // Obtém um grupo de acesso por Nome
        Task<GroupAccess> GetByName(string name);

        // Obtém todos os grupos de acesso
        Task<List<GroupAccess>> GetAll();
    }
}
