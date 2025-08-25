using Meeting.Application.Interfaces;
using Meeting.Domain.Entities;
using Meeting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Meeting.Infrastructure.Repositories
{
    public class FileMetadataRepository : IFileMetadataRepository
    {
        private readonly MeetingDbContext _context;

        public FileMetadataRepository(MeetingDbContext context)
        {
            _context = context;
        }

        public async Task<FileMetadata?> GetFileByIdAsync(int fileId)
        {
            return await _context.FileMetadata
                .Include(f => f.Owner)
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);
        }

        public async Task<FileMetadata?> GetFileByStoredNameAsync(string storedFileName)
        {
            return await _context.FileMetadata
                .Include(f => f.Owner)
                .FirstOrDefaultAsync(f => f.StoredFileName == storedFileName && !f.IsDeleted);
        }

        public async Task<FileMetadata> CreateFileMetadataAsync(FileMetadata fileMetadata)
        {
            _context.FileMetadata.Add(fileMetadata);
            await _context.SaveChangesAsync();
            return fileMetadata;
        }

        public async Task<bool> DeleteFileMetadataAsync(int fileId)
        {
            var file = await _context.FileMetadata.FindAsync(fileId);
            if (file == null) return false;

            file.IsDeleted = true;
            file.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FileMetadata?> UpdateFileMetadataAsync(FileMetadata fileMetadata)
        {
            _context.FileMetadata.Update(fileMetadata);
            await _context.SaveChangesAsync();
            return fileMetadata;
        }

        public async Task<List<FileMetadata>> GetFilesByOwnerAsync(int ownerId, string? fileType = null)
        {
            var query = _context.FileMetadata
                .Where(f => f.OwnerId == ownerId && !f.IsDeleted);

            if (!string.IsNullOrEmpty(fileType))
            {
                query = query.Where(f => f.FileType == fileType);
            }

            return await query.OrderByDescending(f => f.UploadedAt).ToListAsync();
        }

        public async Task<List<FileMetadata>> GetFilesByRelatedEntityAsync(int relatedEntityId, string fileType)
        {
            return await _context.FileMetadata
                .Where(f => f.RelatedEntityId == relatedEntityId && 
                           f.FileType == fileType && 
                           !f.IsDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<bool> CanUserAccessFileAsync(int fileId, int userId)
        {
            var file = await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            if (file == null) return false;

            // Owner can always access
            if (file.OwnerId == userId) return true;

            // For meeting documents, check if user is part of the meeting
            if (file.FileType == FileTypes.MeetingDocument && file.RelatedEntityId.HasValue)
            {
                var meeting = await _context.Meetings
                    .FirstOrDefaultAsync(m => m.Id == file.RelatedEntityId.Value);

                // For now, anyone can access meeting documents of their own meetings
                // In a more complex system, you'd check meeting participants
                return meeting?.UserId == userId;
            }

            return false;
        }

        public async Task<bool> IsFileOwnerAsync(int fileId, int userId)
        {
            var file = await _context.FileMetadata
                .FirstOrDefaultAsync(f => f.Id == fileId && !f.IsDeleted);

            return file?.OwnerId == userId;
        }
    }
}