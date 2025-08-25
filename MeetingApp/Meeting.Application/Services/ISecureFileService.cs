using Meeting.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Meeting.Application.Services
{
    public interface ISecureFileService
    {
        Task<FileMetadata> SaveFileAsync(IFormFile file, int ownerId, string fileType, int? relatedEntityId = null);
        Task<(Stream FileStream, FileMetadata FileInfo)?> GetFileAsync(int fileId, int requestingUserId);
        Task<(Stream FileStream, FileMetadata FileInfo)?> GetFileByNameAsync(string storedFileName, int requestingUserId);
        Task<bool> DeleteFileAsync(int fileId, int requestingUserId);
        Task<bool> ValidateFileAsync(IFormFile file, string fileType);
        Task<List<FileMetadata>> GetUserFilesAsync(int userId, string? fileType = null);
        Task<bool> CanUserAccessFileAsync(int fileId, int userId);
        string GenerateSecureFileName(string originalFileName);
    }
}