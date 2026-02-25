namespace Domain.Interfaces.Repository
{
    public interface IComplaintRepository
    {
        Task Add(Complaint complaint);
        Task RemoveById(Guid complaintId);
        Task Update(Complaint complaint);
        Task<Complaint> GetById(Guid complaintId);
        Task<List<Complaint>> Get(int page, int pageSize);
    }
}
