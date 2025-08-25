using Meeting.Application.Services;
using Meeting.Domain.Entities;

namespace Meeting.Api.Services
{
    /// <summary>
    /// Legacy file storage service - DEPRECATED
    /// Use ISecureFileService instead for new implementations
    /// This service is kept for backward compatibility only
    /// </summary>
    [Obsolete("Use ISecureFileService instead. This service will be removed in future versions.")]
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
    }

    [Obsolete("Use SecureFileService instead. This service will be removed in future versions.")]
    public class FileStorageService : IFileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly ISecureFileService _secureFileService;

        public FileStorageService(ILogger<FileStorageService> logger, ISecureFileService secureFileService)
        {
            _logger = logger;
            _secureFileService = secureFileService;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            _logger.LogWarning("Using deprecated FileStorageService. Please migrate to SecureFileService.");
            
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is null or empty");

                // For backward compatibility, try to determine file type from folder
                var fileType = folder switch
                {
                    "profiles" => FileTypes.ProfileImage,
                    "documents" => FileTypes.MeetingDocument,
                    _ => FileTypes.MeetingDocument
                };

                // This is unsafe - we don't have user context in legacy service
                // In a real migration, you'd need to pass userId or refactor calling code
                var defaultUserId = 1; // This should be replaced with actual user ID
                
                var fileMetadata = await _secureFileService.SaveFileAsync(file, defaultUserId, fileType);
                return fileMetadata.StoredFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file using legacy service");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            _logger.LogWarning("Using deprecated FileStorageService.DeleteFileAsync. This method is not secure.");
            
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                // Legacy method - cannot determine file ID or user context
                // This is why the old service is insecure
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file using legacy service");
                return false;
            }
        }
    }
}