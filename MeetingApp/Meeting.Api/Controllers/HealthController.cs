using Meeting.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Meeting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                // In a real implementation, you would check database connectivity,
                // external service availability, etc.
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0"
                };

                return Ok(ApiResponse<object>.SuccessResponse(healthStatus, "Service is healthy"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("Health check failed"));
            }
        }
    }
}