namespace Spendnt.API.Helpers
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(byte[] content, string extension, string containerName);
        Task DeleteFileAsync(string filePath, string containerName);
        
    }
}