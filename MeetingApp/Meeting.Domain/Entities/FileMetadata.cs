using System.ComponentModel.DataAnnotations;

namespace Meeting.Domain.Entities
{
    public class FileMetadata
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public required string OriginalFileName { get; set; }
        
        [Required]
        [MaxLength(255)]
        public required string StoredFileName { get; set; }
        
        [Required]
        [MaxLength(500)]
        public required string FilePath { get; set; }
        
        [Required]
        [MaxLength(10)]
        public required string FileExtension { get; set; }
        
        [Required]
        public long FileSize { get; set; }
        
        public long? CompressedSize { get; set; }
        
        public bool IsCompressed { get; set; } = false;
        
        [MaxLength(20)]
        public string? CompressionType { get; set; } // "gzip", "brotli", etc.
        
        [Required]
        [MaxLength(50)]
        public required string ContentType { get; set; }
        
        [Required]
        public int OwnerId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public required string FileType { get; set; } // "ProfileImage", "MeetingDocument"
        
        public int? RelatedEntityId { get; set; } // Meeting ID for documents
        
        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastAccessedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public User? Owner { get; set; }
    }
    
    public static class FileTypes
    {
        public const string ProfileImage = "ProfileImage";
        public const string MeetingDocument = "MeetingDocument";
    }
    
    public static class AllowedExtensions
    {
        public static readonly Dictionary<string, string[]> ByFileType = new()
        {
            [FileTypes.ProfileImage] = new[] { ".jpg", ".jpeg", ".png", ".gif" },
            [FileTypes.MeetingDocument] = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".jpg", ".jpeg", ".png", ".gif" }
        };
        
        public static readonly Dictionary<string, string[]> ContentTypes = new()
        {
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" },
            [".gif"] = new[] { "image/gif" },
            [".pdf"] = new[] { "application/pdf" },
            [".doc"] = new[] { "application/msword" },
            [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            [".xls"] = new[] { "application/vnd.ms-excel" },
            [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            [".ppt"] = new[] { "application/vnd.ms-powerpoint" },
            [".pptx"] = new[] { "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            [".txt"] = new[] { "text/plain" }
        };
    }
}