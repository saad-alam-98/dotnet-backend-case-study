namespace SaudiPlus.Interfaces
{
    public interface IFileService
    {
        Task<(string URL, Exception? error)> UploadFileAsync(IFormFile file, string pathPrefix, int? sizeLimitInMB = null);
        Task<(bool success, Exception? error)> DeleteFileAsync(string key);
        IFormFile CreateFormFileFromBytes(byte[] fileBytes, string fileName, string contentType);
    }
}
