namespace Domain.Interfaces.Services
{
    public interface IS3Service
    {
        Task<string> GetFileUrlByFileNameKey(string fileNameKey);

        Task<string> SendStreamFileToS3(Stream stream, string extension);

        Task DeleteFileByFileNameKey(string fileNameKey);
    }
}
