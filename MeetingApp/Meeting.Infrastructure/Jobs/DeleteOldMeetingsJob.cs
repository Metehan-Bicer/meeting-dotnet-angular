using Hangfire;
using Hangfire.Server;
using Meeting.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Meeting.Infrastructure.Jobs
{
    public class DeleteOldMeetingsJob
    {
        private readonly IMeetingService _meetingService;

        public DeleteOldMeetingsJob(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        public static void ScheduleRecurringJob()
        {
            // Schedule job to run daily at 2:00 AM
            RecurringJob.AddOrUpdate<DeleteOldMeetingsJob>(
                job => job.DeleteOldCancelledMeetings(null),
                "0 2 * * *",
                TimeZoneInfo.Utc);
        }

        public async Task DeleteOldCancelledMeetings(PerformContext? context)
        {
            try
            {
                await _meetingService.DeleteOldCancelledMeetingsAsync();
            }
            catch (Exception ex)
            {
                // Log the exception
                context?.SetJobParameter("Exception", ex.Message);
                throw;
            }
        }
    }
}