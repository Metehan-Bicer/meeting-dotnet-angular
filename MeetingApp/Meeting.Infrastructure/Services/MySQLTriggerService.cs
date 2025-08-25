using Meeting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meeting.Infrastructure.Services
{
    public interface IMySQLTriggerService
    {
        Task CreateMeetingDeleteTriggerAsync();
        Task DropMeetingDeleteTriggerAsync();
    }

    public class MySQLTriggerService : IMySQLTriggerService
    {
        private readonly MeetingDbContext _context;
        private readonly ILogger<MySQLTriggerService> _logger;

        public MySQLTriggerService(MeetingDbContext context, ILogger<MySQLTriggerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateMeetingDeleteTriggerAsync()
        {
            try
            {
                var triggerSql = @"
                    CREATE TRIGGER TR_Meetings_Delete
                    AFTER DELETE ON Meetings
                    FOR EACH ROW
                    BEGIN
                        INSERT INTO MeetingDeleteLogs (
                            MeetingId, Title, Description, StartDate, EndDate, DocumentPath,
                            UserId, UserName, UserEmail, DeletedAt, DeletedBy, DeleteReason,
                            WasCancelled, CancelledAt, OriginalCreatedAt
                        ) VALUES (
                            OLD.Id,
                            OLD.Title,
                            OLD.Description,
                            OLD.StartDate,
                            OLD.EndDate,
                            OLD.DocumentPath,
                            OLD.UserId,
                            IFNULL(
                                (SELECT CONCAT(u.FirstName, ' ', u.LastName) 
                                 FROM Users u WHERE u.Id = OLD.UserId),
                                'Unknown User'
                            ),
                            IFNULL(
                                (SELECT u.Email FROM Users u WHERE u.Id = OLD.UserId),
                                'unknown@email.com'
                            ),
                            UTC_TIMESTAMP(),
                            'SYSTEM',
                            CASE 
                                WHEN OLD.IsCancelled = 1 THEN 'Auto-cleanup of cancelled meeting'
                                ELSE 'Manual deletion'
                            END,
                            OLD.IsCancelled,
                            OLD.CancelledAt,
                            OLD.CreatedAt
                        );
                    END";

                await _context.Database.ExecuteSqlRawAsync(triggerSql);
                _logger.LogInformation("MySQL trigger TR_Meetings_Delete created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MySQL trigger");
                throw;
            }
        }

        public async Task DropMeetingDeleteTriggerAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS TR_Meetings_Delete");
                _logger.LogInformation("MySQL trigger TR_Meetings_Delete dropped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop MySQL trigger");
                throw;
            }
        }
    }
}