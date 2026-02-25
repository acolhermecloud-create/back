namespace Domain.Interfaces.Repository
{
    public interface ICategoryRepository
    {
        // Adiciona uma nova categoria
        Task Add(Category category);

        // Atualiza uma categoria existente
        Task Update(Category category);

        // Remove uma categoria pelo ID
        Task Delete(Guid id);

        // Busca uma categoria pelo ID
        Task<Category> GetById(Guid id);

        // Retorna todas as categorias
        Task<List<Category>> GetAll();
    }
}
