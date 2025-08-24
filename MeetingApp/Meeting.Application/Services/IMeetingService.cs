using Meeting.Domain.DTOs;
using Meeting.Domain.Entities;

namespace Meeting.Application.Services
{
    public interface IMeetingService
    {
        Task<MeetingEntity?> CreateMeetingAsync(MeetingCreateDto meetingDto, int userId, string? documentPath);
        Task<MeetingEntity?> UpdateMeetingAsync(int meetingId, MeetingUpdateDto meetingDto);
        Task<bool> CancelMeetingAsync(int meetingId);
        Task<IEnumerable<MeetingEntity>> GetUserMeetingsAsync(int userId);
        Task<IEnumerable<MeetingEntity>> GetAllMeetingsAsync();
        Task<MeetingEntity?> GetMeetingByIdAsync(int id);
        Task<bool> DeleteOldCancelledMeetingsAsync();
    }
}