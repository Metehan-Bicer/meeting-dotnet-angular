using Hangfire;
using Hangfire.MemoryStorage;
using Meeting.Api.Extensions;
using Meeting.Api.Middleware;
using Meeting.Api.Services;
using Meeting.Infrastructure.BackgroundServices;
using Meeting.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add custom extensions
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHangfireServices(builder.Configuration);

// Add JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// Add File Storage service
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// Add background service
builder.Services.AddHostedService<DeleteOldMeetingsService>();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    // Configure JWT Bearer options
    options.Authority = "https://localhost:5001";
    options.Audience = "api1";
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters.ValidateAudience = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add CORS
app.UseCors("AllowAll");

// Add global exception handler
app.UseExceptionHandler("/error");
// Note: The exact syntax for this may vary depending on .NET version
// For now, we'll use the standard exception handler
app.UseExceptionHandler(builder =>
{
    builder.Run(async context =>
    {
        var exceptionHandler = app.Services.GetRequiredService<GlobalExceptionHandler>();
        await exceptionHandler.InvokeAsync(context);
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire dashboard
app.UseHangfireDashboard();

// Map health checks
app.MapHealthChecks("/health");

// Schedule recurring jobs
if (app.Environment.IsDevelopment())
{
    // In development, we'll run the job immediately for testing
    using var scope = app.Services.CreateScope();
    var meetingService = scope.ServiceProvider.GetRequiredService<Meeting.Application.Services.IMeetingService>();
    // Fire and forget the job for testing
    _ = meetingService.DeleteOldCancelledMeetingsAsync();
}
else
{
    DeleteOldMeetingsJob.ScheduleRecurringJob();
}

app.MapControllers();

app.Run();