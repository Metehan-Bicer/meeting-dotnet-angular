using Meeting.Application.Interfaces;
using Meeting.Application.Services;
using Meeting.Domain.DTOs;
using Meeting.Domain.Entities;

namespace Meeting.Application.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IUserRepository _userRepository;

        public MeetingService(IMeetingRepository meetingRepository, IUserRepository userRepository)
        {
            _meetingRepository = meetingRepository;
            _userRepository = userRepository;
        }

        public async Task<MeetingEntity?> CreateMeetingAsync(MeetingCreateDto meetingDto, int userId, string? documentPath)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var meeting = new MeetingEntity
            {
                Title = meetingDto.Title,
                Description = meetingDto.Description,
                StartDate = meetingDto.StartDate,
                EndDate = meetingDto.EndDate,
                DocumentPath = documentPath,
                UserId = userId
            };

            return await _meetingRepository.CreateMeetingAsync(meeting);
        }

        public async Task<MeetingEntity?> UpdateMeetingAsync(int meetingId, MeetingUpdateDto meetingDto)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
            {
                return null;
            }

            meeting.Title = meetingDto.Title;
            meeting.Description = meetingDto.Description;
            meeting.StartDate = meetingDto.StartDate;
            meeting.EndDate = meetingDto.EndDate;
            
            // Only update cancelled status if it's being set to true
            if (meetingDto.IsCancelled && !meeting.IsCancelled)
            {
                meeting.IsCancelled = true;
                meeting.CancelledAt = DateTime.UtcNow;
            }

            return await _meetingRepository.UpdateMeetingAsync(meeting);
        }

        public async Task<bool> CancelMeetingAsync(int meetingId)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(meetingId);
            if (meeting == null)
            {
                return false;
            }

            meeting.IsCancelled = true;
            meeting.CancelledAt = DateTime.UtcNow;

            await _meetingRepository.UpdateMeetingAsync(meeting);
            return true;
        }

        public async Task<IEnumerable<MeetingEntity>> GetUserMeetingsAsync(int userId)
        {
            return await _meetingRepository.GetUserMeetingsAsync(userId);
        }

        public async Task<IEnumerable<MeetingEntity>> GetAllMeetingsAsync()
        {
            return await _meetingRepository.GetAllMeetingsAsync();
        }

        public async Task<MeetingEntity?> GetMeetingByIdAsync(int id)
        {
            return await _meetingRepository.GetMeetingByIdAsync(id);
        }

        public async Task<bool> DeleteOldCancelledMeetingsAsync()
        {
            var oldCancelledMeetings = await _meetingRepository.GetCancelledMeetingsAsync();
            
            foreach (var meeting in oldCancelledMeetings)
            {
                await _meetingRepository.DeleteMeetingAsync(meeting.Id);
            }
            
            return true;
        }
    }
}