using Meeting.Domain.Entities;

namespace Meeting.Application.Interfaces
{
    public interface IMeetingRepository
    {
        Task<MeetingEntity?> GetMeetingByIdAsync(int id);
        Task<IEnumerable<MeetingEntity>> GetAllMeetingsAsync();
        Task<IEnumerable<MeetingEntity>> GetUserMeetingsAsync(int userId);
        Task<MeetingEntity> CreateMeetingAsync(MeetingEntity meeting);
        Task<MeetingEntity> UpdateMeetingAsync(MeetingEntity meeting);
        Task DeleteMeetingAsync(int id);
        Task<IEnumerable<MeetingEntity>> GetCancelledMeetingsAsync();
    }
}