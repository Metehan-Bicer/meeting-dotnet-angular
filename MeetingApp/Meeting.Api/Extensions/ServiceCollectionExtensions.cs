using Hangfire;
using Hangfire.MemoryStorage;
using Meeting.Application.Interfaces;
using Meeting.Application.Services;
using Meeting.Infrastructure.Data;
using Meeting.Infrastructure.Jobs;
using Meeting.Infrastructure.Repositories;
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

            // Add services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMeetingService, MeetingService>();

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
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                // Use SQLite database for development
                var dbPath = Path.Combine(dbDirectory, "MeetingApp.db");
                services.AddDbContext<MeetingDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
            }
            else
            {
                // Use SQLite for production as well (simplifying for this example)
                var dbPath = Path.Combine(dbDirectory, "MeetingApp.db");
                services.AddDbContext<MeetingDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));
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