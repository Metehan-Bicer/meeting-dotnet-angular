using Meeting.Domain.Entities;

namespace Meeting.Application.Interfaces
{
    public interface IFileMetadataRepository
    {
        Task<FileMetadata?> GetFileByIdAsync(int fileId);
        Task<FileMetadata?> GetFileByStoredNameAsync(string storedFileName);
        Task<FileMetadata> CreateFileMetadataAsync(FileMetadata fileMetadata);
        Task<bool> DeleteFileMetadataAsync(int fileId);
        Task<FileMetadata?> UpdateFileMetadataAsync(FileMetadata fileMetadata);
        Task<List<FileMetadata>> GetFilesByOwnerAsync(int ownerId, string? fileType = null);
        Task<List<FileMetadata>> GetFilesByRelatedEntityAsync(int relatedEntityId, string fileType);
        Task<bool> CanUserAccessFileAsync(int fileId, int userId);
        Task<bool> IsFileOwnerAsync(int fileId, int userId);
    }
}