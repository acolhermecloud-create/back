namespace Domain.Interfaces.Repository
{
    public interface IUtmRepository
    {
        Task Add(Utm utm);

        Task Update(Utm utm);

        Task<Utm> GetByOrderId(string orderId);
    }
}
