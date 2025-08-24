using Meeting.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meeting.Infrastructure.BackgroundServices
{
    public class DeleteOldMeetingsService : BackgroundService
    {
        private readonly ILogger<DeleteOldMeetingsService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DeleteOldMeetingsService(ILogger<DeleteOldMeetingsService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Delete Old Meetings Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var meetingService = scope.ServiceProvider.GetRequiredService<IMeetingService>();
                        await meetingService.DeleteOldCancelledMeetingsAsync();
                    }

                    _logger.LogInformation("Old cancelled meetings deleted successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while deleting old cancelled meetings.");
                }

                // Run every day
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delete Old Meetings Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}