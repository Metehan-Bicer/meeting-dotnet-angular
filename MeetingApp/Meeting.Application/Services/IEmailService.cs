using Meeting.Domain.Entities;

namespace Meeting.Application.Services
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(User user);
        Task SendMeetingNotificationEmailAsync(User user, MeetingEntity meeting, string notificationType);
        Task SendMeetingReminderEmailAsync(User user, MeetingEntity meeting);
        Task SendMeetingCancellationEmailAsync(User user, MeetingEntity meeting);
    }
}