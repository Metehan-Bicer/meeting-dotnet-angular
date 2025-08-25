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
        private readonly IEmailService _emailService;

        public MeetingService(IMeetingRepository meetingRepository, IUserRepository userRepository, IEmailService emailService)
        {
            _meetingRepository = meetingRepository;
            _userRepository = userRepository;
            _emailService = emailService;
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

            var createdMeeting = await _meetingRepository.CreateMeetingAsync(meeting);
            
            // Send meeting notification email
            if (createdMeeting != null)
            {
                try
                {
                    await _emailService.SendMeetingNotificationEmailAsync(user, createdMeeting, "Oluşturuldu");
                }
                catch (Exception)
                {
                    // Log but don't fail the meeting creation if email fails
                }
            }

            return createdMeeting;
        }

        public async Task<MeetingEntity?> UpdateMeetingAsync(int meetingId, MeetingUpdateDto meetingDto, string? documentPath = null)
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
            
            // Update document path if provided
            if (documentPath != null)
            {
                meeting.DocumentPath = documentPath;
            }
            
            // Only update cancelled status if it's being set to true
            if (meetingDto.IsCancelled && !meeting.IsCancelled)
            {
                meeting.IsCancelled = true;
                meeting.CancelledAt = DateTime.UtcNow;
                
                // Send cancellation email
                var user = await _userRepository.GetUserByIdAsync(meeting.UserId);
                if (user != null)
                {
                    try
                    {
                        await _emailService.SendMeetingCancellationEmailAsync(user, meeting);
                    }
                    catch (Exception)
                    {
                        // Log but don't fail the update if email fails
                    }
                }
            }

            var updatedMeeting = await _meetingRepository.UpdateMeetingAsync(meeting);
            
            // Send update notification email if not cancelled
            if (updatedMeeting != null && !meetingDto.IsCancelled)
            {
                var user = await _userRepository.GetUserByIdAsync(meeting.UserId);
                if (user != null)
                {
                    try
                    {
                        await _emailService.SendMeetingNotificationEmailAsync(user, updatedMeeting, "Güncellendi");
                    }
                    catch (Exception)
                    {
                        // Log but don't fail the update if email fails
                    }
                }
            }

            return updatedMeeting;
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