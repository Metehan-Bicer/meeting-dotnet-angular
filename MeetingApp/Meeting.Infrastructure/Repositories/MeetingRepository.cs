using Meeting.Application.Interfaces;
using Meeting.Domain.Entities;
using Meeting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Meeting.Infrastructure.Repositories
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly MeetingDbContext _context;

        public MeetingRepository(MeetingDbContext context)
        {
            _context = context;
        }

        public async Task<MeetingEntity?> GetMeetingByIdAsync(int id)
        {
            return await _context.Meetings.FindAsync(id);
        }

        public async Task<IEnumerable<MeetingEntity>> GetAllMeetingsAsync()
        {
            return await _context.Meetings
                .Where(m => !m.IsCancelled)
                .ToListAsync();
        }

        public async Task<IEnumerable<MeetingEntity>> GetUserMeetingsAsync(int userId)
        {
            return await _context.Meetings
                .Where(m => m.UserId == userId && !m.IsCancelled)
                .ToListAsync();
        }

        public async Task<MeetingEntity> CreateMeetingAsync(MeetingEntity meeting)
        {
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }

        public async Task<MeetingEntity> UpdateMeetingAsync(MeetingEntity meeting)
        {
            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }

        public async Task DeleteMeetingAsync(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting != null)
            {
                _context.Meetings.Remove(meeting);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MeetingEntity>> GetCancelledMeetingsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30); // Example: delete meetings cancelled more than 30 days ago
            return await _context.Meetings
                .Where(m => m.IsCancelled && m.CancelledAt <= cutoffDate)
                .ToListAsync();
        }
    }
}