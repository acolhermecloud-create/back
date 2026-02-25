namespace Domain.Interfaces.Repository
{
    public interface IAddressRepository
    {
        // Adiciona um novo endereço
        Task Add(Address address);

        // Atualiza um endereço existente
        Task Update(Address address);

        // Remove um endereço pelo ID
        Task Delete(Guid id);

        // Busca um endereço pelo ID
        Task<Address> GetById(Guid id);
    }
}
