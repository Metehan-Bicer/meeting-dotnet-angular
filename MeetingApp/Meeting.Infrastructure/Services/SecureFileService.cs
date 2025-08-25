using Meeting.Application.Interfaces;
using Meeting.Application.Services;
using Meeting.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace Meeting.Infrastructure.Services
{
    public class SecureFileService : ISecureFileService
    {
        private readonly IFileMetadataRepository _fileRepository;
        private readonly ILogger<SecureFileService> _logger;
        private readonly IFileCompressionService _compressionService;
        private readonly string _uploadsPath;
        private const int MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public SecureFileService(IFileMetadataRepository fileRepository, ILogger<SecureFileService> logger, IFileCompressionService compressionService)
        {
            _fileRepository = fileRepository;
            _logger = logger;
            _compressionService = compressionService;
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "secure-uploads");
            EnsureDirectoryExists(_uploadsPath);
        }

        public async Task<FileMetadata> SaveFileAsync(IFormFile file, int ownerId, string fileType, int? relatedEntityId = null)
        {
            if (!await ValidateFileAsync(file, fileType))
            {
                throw new InvalidOperationException("Invalid file type or size");
            }

            var storedFileName = GenerateSecureFileName(file.FileName);
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            // Create secure folder structure: uploads/{fileType}/{ownerId}/
            var folderPath = Path.Combine(_uploadsPath, fileType, ownerId.ToString());
            EnsureDirectoryExists(folderPath);

            var filePath = Path.Combine(folderPath, storedFileName);

            // Compress and save file to disk
            using (var inputStream = file.OpenReadStream())
            {
                var (processedStream, processedSize, compressionType) = await _compressionService.CompressFileAsync(inputStream, file.FileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                using (processedStream)
                {
                    await processedStream.CopyToAsync(fileStream);
                }

                // Create metadata record with compression info
                var fileMetadata = new FileMetadata
                {
                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,
                    FilePath = filePath,
                    FileExtension = fileExtension,
                    FileSize = file.Length,
                    CompressedSize = processedSize,
                    IsCompressed = compressionType != "none",
                    CompressionType = compressionType,
                    ContentType = file.ContentType,
                    OwnerId = ownerId,
                    FileType = fileType,
                    RelatedEntityId = relatedEntityId,
                    UploadedAt = DateTime.UtcNow
                };

                var savedFile = await _fileRepository.CreateFileMetadataAsync(fileMetadata);
                
                if (fileMetadata.IsCompressed)
                {
                    _logger.LogInformation("File compressed and saved: {FileName} for user {UserId}. Size: {OriginalSize} -> {CompressedSize} bytes", 
                        file.FileName, ownerId, file.Length, processedSize);
                }
                else
                {
                    _logger.LogInformation("File saved without compression: {FileName} for user {UserId}", file.FileName, ownerId);
                }

                return savedFile;
            }
        }

        public async Task<(Stream FileStream, FileMetadata FileInfo)?> GetFileAsync(int fileId, int requestingUserId)
        {
            if (!await _fileRepository.CanUserAccessFileAsync(fileId, requestingUserId))
            {
                _logger.LogWarning("Unauthorized file access attempt: FileId {FileId}, UserId {UserId}", fileId, requestingUserId);
                return null;
            }

            var fileMetadata = await _fileRepository.GetFileByIdAsync(fileId);
            if (fileMetadata == null || !File.Exists(fileMetadata.FilePath))
            {
                return null;
            }

            // Update last accessed time
            fileMetadata.LastAccessedAt = DateTime.UtcNow;
            await _fileRepository.UpdateFileMetadataAsync(fileMetadata);

            // Read and decompress file if needed
            var fileStream = new FileStream(fileMetadata.FilePath, FileMode.Open, FileAccess.Read);
            
            if (fileMetadata.IsCompressed && !string.IsNullOrEmpty(fileMetadata.CompressionType))
            {
                var decompressedStream = await _compressionService.DecompressFileAsync(fileStream, fileMetadata.CompressionType);
                fileStream.Dispose(); // Dispose original compressed stream
                
                return (decompressedStream, fileMetadata);
            }

            return (fileStream, fileMetadata);
        }

        public async Task<(Stream FileStream, FileMetadata FileInfo)?> GetFileByNameAsync(string storedFileName, int requestingUserId)
        {
            var fileMetadata = await _fileRepository.GetFileByStoredNameAsync(storedFileName);
            if (fileMetadata == null)
            {
                return null;
            }

            return await GetFileAsync(fileMetadata.Id, requestingUserId);
        }

        public async Task<bool> DeleteFileAsync(int fileId, int requestingUserId)
        {
            if (!await _fileRepository.IsFileOwnerAsync(fileId, requestingUserId))
            {
                _logger.LogWarning("Unauthorized file deletion attempt: FileId {FileId}, UserId {UserId}", fileId, requestingUserId);
                return false;
            }

            var fileMetadata = await _fileRepository.GetFileByIdAsync(fileId);
            if (fileMetadata == null)
            {
                return false;
            }

            // Delete physical file
            if (File.Exists(fileMetadata.FilePath))
            {
                File.Delete(fileMetadata.FilePath);
            }

            // Mark as deleted in database
            await _fileRepository.DeleteFileMetadataAsync(fileId);
            
            _logger.LogInformation("File deleted: FileId {FileId} by UserId {UserId}", fileId, requestingUserId);
            return true;
        }

        public async Task<bool> ValidateFileAsync(IFormFile file, string fileType)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("File too large: {FileSize} bytes, max allowed: {MaxSize}", file.Length, MaxFileSizeBytes);
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.ByFileType.TryGetValue(fileType, out var allowedExtensions) ||
                !allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Invalid file extension: {Extension} for file type: {FileType}", extension, fileType);
                return false;
            }

            // Check content type
            if (!AllowedExtensions.ContentTypes.TryGetValue(extension, out var allowedContentTypes) ||
                !allowedContentTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("Invalid content type: {ContentType} for extension: {Extension}", file.ContentType, extension);
                return false;
            }

            // Additional security check: verify file signature (magic numbers)
            if (!await ValidateFileSignatureAsync(file, extension))
            {
                _logger.LogWarning("File signature validation failed for: {FileName}", file.FileName);
                return false;
            }

            return true;
        }

        public async Task<List<FileMetadata>> GetUserFilesAsync(int userId, string? fileType = null)
        {
            return await _fileRepository.GetFilesByOwnerAsync(userId, fileType);
        }

        public async Task<bool> CanUserAccessFileAsync(int fileId, int userId)
        {
            return await _fileRepository.CanUserAccessFileAsync(fileId, userId);
        }

        public string GenerateSecureFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var randomPart = GenerateRandomString(8);
            
            return $"{timestamp}_{randomPart}{extension}";
        }

        private async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension)
        {
            // Read first few bytes to validate file signature
            var buffer = new byte[8];
            using var stream = file.OpenReadStream();
            await stream.ReadAsync(buffer, 0, 8);
            stream.Seek(0, SeekOrigin.Begin); // Reset stream position

            return extension switch
            {
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
                ".pdf" => buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46,
                ".gif" => (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46),
                _ => true // For other types, we rely on extension and content type validation
            };
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using var rng = RandomNumberGenerator.Create();
            var result = new StringBuilder(length);
            var buffer = new byte[4];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buffer);
                var randomIndex = BitConverter.ToUInt32(buffer, 0) % chars.Length;
                result.Append(chars[(int)randomIndex]);
            }

            return result.ToString();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}