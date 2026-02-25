namespace Domain.Interfaces.Repository
{
    public interface IOngRepository
    {
        Task Add(Ong ong);
        Task Update(Ong ong);
        Task Delete(Guid id);
        Task<Ong> GetById(Guid id);
        Task<List<Ong>> GetAll(int page, int pageSize);
    }
}