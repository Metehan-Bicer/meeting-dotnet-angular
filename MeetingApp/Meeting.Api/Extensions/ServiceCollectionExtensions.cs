using Hangfire;
using Hangfire.MemoryStorage;
using Meeting.Application.Interfaces;
using Meeting.Application.Services;
using Meeting.Infrastructure.Data;
using Meeting.Infrastructure.Jobs;
using Meeting.Infrastructure.Repositories;
using Meeting.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Meeting.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<IFileMetadataRepository, FileMetadataRepository>();

            // Add services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileCompressionService, FileCompressionService>();
            services.AddScoped<ISecureFileService, SecureFileService>();
            services.AddScoped<IMySQLTriggerService, MySQLTriggerService>();

            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Ensure database directory exists
            var dbDirectory = Path.Combine(Directory.GetCurrentDirectory(), "database");
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            // Add Entity Framework
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to SQLite if no connection string is configured
                var dbPath = Path.Combine(dbDirectory, "MeetingApp.db");
                services.AddDbContext<MeetingDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
            }
            else if (connectionString.Contains("3306") || connectionString.Contains("mysql", StringComparison.OrdinalIgnoreCase))
            {
                // Use MySQL
                services.AddDbContext<MeetingDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            }
            else
            {
                // Use SQL Server
                services.AddDbContext<MeetingDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            return services;
        }

        public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Hangfire services
            // Use in-memory storage for both development and production
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage());

            // Add Hangfire server
            services.AddHangfireServer();

            return services;
        }
    }
}