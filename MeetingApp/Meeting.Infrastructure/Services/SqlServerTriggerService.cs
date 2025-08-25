using Meeting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meeting.Infrastructure.Services
{
    public interface ISqlServerTriggerService
    {
        Task CreateMeetingDeleteTriggerAsync();
        Task DropMeetingDeleteTriggerAsync();
    }

    public class SqlServerTriggerService : ISqlServerTriggerService
    {
        private readonly MeetingDbContext _context;
        private readonly ILogger<SqlServerTriggerService> _logger;

        public SqlServerTriggerService(MeetingDbContext context, ILogger<SqlServerTriggerService> logger)
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
                    ON Meetings
                    AFTER DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        
                        INSERT INTO MeetingDeleteLogs (
                            MeetingId, Title, Description, StartDate, EndDate, DocumentPath,
                            UserId, UserName, UserEmail, DeletedAt, DeletedBy, DeleteReason,
                            WasCancelled, CancelledAt, OriginalCreatedAt
                        )
                        SELECT 
                            d.Id,
                            d.Title,
                            d.Description,
                            d.StartDate,
                            d.EndDate,
                            d.DocumentPath,
                            d.UserId,
                            COALESCE(u.FirstName + ' ' + u.LastName, 'Unknown User'),
                            COALESCE(u.Email, 'unknown@email.com'),
                            GETUTCDATE(),
                            'SYSTEM',
                            CASE 
                                WHEN d.IsCancelled = 1 THEN 'Auto-cleanup of cancelled meeting'
                                ELSE 'Manual deletion'
                            END,
                            d.IsCancelled,
                            d.CancelledAt,
                            d.CreatedAt
                        FROM deleted d
                        LEFT JOIN Users u ON d.UserId = u.Id;
                    END";

                await _context.Database.ExecuteSqlRawAsync(triggerSql);
                _logger.LogInformation("SQL Server trigger TR_Meetings_Delete created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SQL Server trigger");
                throw;
            }
        }

        public async Task DropMeetingDeleteTriggerAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS TR_Meetings_Delete");
                _logger.LogInformation("SQL Server trigger TR_Meetings_Delete dropped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drop SQL Server trigger");
                throw;
            }
        }
    }
}