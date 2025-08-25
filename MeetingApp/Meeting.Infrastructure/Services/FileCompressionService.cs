using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Meeting.Infrastructure.Services
{
    public interface IFileCompressionService
    {
        Task<(Stream CompressedStream, long CompressedSize, string CompressionType)> CompressFileAsync(Stream inputStream, string fileName);
        Task<Stream> DecompressFileAsync(Stream compressedStream, string compressionType);
        bool ShouldCompressFile(string fileName, long fileSize);
    }

    public class FileCompressionService : IFileCompressionService
    {
        private readonly ILogger<FileCompressionService> _logger;
        
        // File types that benefit from compression
        private static readonly HashSet<string> CompressibleTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".json", ".xml", ".html", ".css", ".js",
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".pdf", ".rtf", ".odt", ".ods", ".odp"
        };

        // File types that are already compressed (don't compress these)
        private static readonly HashSet<string> AlreadyCompressedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".mp3", ".mp4", ".avi", 
            ".zip", ".rar", ".7z", ".gz", ".bz2"
        };

        // Minimum file size to consider compression (1KB)
        private const long MinCompressionSize = 1024;

        public FileCompressionService(ILogger<FileCompressionService> logger)
        {
            _logger = logger;
        }

        public async Task<(Stream CompressedStream, long CompressedSize, string CompressionType)> CompressFileAsync(Stream inputStream, string fileName)
        {
            var extension = Path.GetExtension(fileName);
            
            if (!ShouldCompressFile(fileName, inputStream.Length))
            {
                // Return original stream if compression is not beneficial
                inputStream.Seek(0, SeekOrigin.Begin);
                return (inputStream, inputStream.Length, "none");
            }

            var compressionType = "gzip";
            var compressedStream = new MemoryStream();

            try
            {
                inputStream.Seek(0, SeekOrigin.Begin);

                // Use GZip compression
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
                {
                    await inputStream.CopyToAsync(gzipStream);
                }

                var compressedSize = compressedStream.Length;
                
                // Check if compression actually reduced file size significantly (at least 10% reduction)
                if (compressedSize >= inputStream.Length * 0.9)
                {
                    _logger.LogInformation("Compression not beneficial for {FileName}. Original: {OriginalSize}, Compressed: {CompressedSize}", 
                        fileName, inputStream.Length, compressedSize);
                    
                    // Return original stream
                    inputStream.Seek(0, SeekOrigin.Begin);
                    compressedStream.Dispose();
                    return (inputStream, inputStream.Length, "none");
                }

                compressedStream.Seek(0, SeekOrigin.Begin);
                
                _logger.LogInformation("File compressed successfully. {FileName}: {OriginalSize} -> {CompressedSize} bytes ({Ratio:P1} reduction)", 
                    fileName, inputStream.Length, compressedSize, 1 - (double)compressedSize / inputStream.Length);

                return (compressedStream, compressedSize, compressionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compress file {FileName}", fileName);
                compressedStream.Dispose();
                
                // Return original stream on compression failure
                inputStream.Seek(0, SeekOrigin.Begin);
                return (inputStream, inputStream.Length, "none");
            }
        }

        public async Task<Stream> DecompressFileAsync(Stream compressedStream, string compressionType)
        {
            if (compressionType == "none" || string.IsNullOrEmpty(compressionType))
            {
                compressedStream.Seek(0, SeekOrigin.Begin);
                return compressedStream;
            }

            var decompressedStream = new MemoryStream();

            try
            {
                compressedStream.Seek(0, SeekOrigin.Begin);

                switch (compressionType.ToLowerInvariant())
                {
                    case "gzip":
                        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress, leaveOpen: true))
                        {
                            await gzipStream.CopyToAsync(decompressedStream);
                        }
                        break;

                    default:
                        _logger.LogWarning("Unknown compression type: {CompressionType}", compressionType);
                        await compressedStream.CopyToAsync(decompressedStream);
                        break;
                }

                decompressedStream.Seek(0, SeekOrigin.Begin);
                return decompressedStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decompress file with compression type {CompressionType}", compressionType);
                decompressedStream.Dispose();
                
                // Return original stream on decompression failure
                compressedStream.Seek(0, SeekOrigin.Begin);
                return compressedStream;
            }
        }

        public bool ShouldCompressFile(string fileName, long fileSize)
        {
            var extension = Path.GetExtension(fileName);

            // Don't compress files that are too small
            if (fileSize < MinCompressionSize)
            {
                return false;
            }

            // Don't compress already compressed file types
            if (AlreadyCompressedTypes.Contains(extension))
            {
                return false;
            }

            // Compress text-based and office document files
            return CompressibleTypes.Contains(extension) || fileSize > 100 * 1024; // Compress files > 100KB regardless of type
        }
    }
}