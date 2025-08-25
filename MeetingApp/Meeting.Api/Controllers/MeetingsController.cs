using Meeting.Api.Models;
using Meeting.Api.Services;
using Meeting.Application.Services;
using Meeting.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Meeting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MeetingsController> _logger;

        public MeetingsController(
            IMeetingService meetingService, 
            IFileStorageService fileStorageService,
            ILogger<MeetingsController> logger)
        {
            _meetingService = meetingService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromForm] MeetingCreateDto meetingDto, IFormFile? document)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<object>.ErrorResponse(errors, "Validation failed"));
                }

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("id")?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    // Fallback to hard-coded for development
                    userId = 1;
                }

                // Save document if provided
                string? documentPath = null;
                if (document != null && document.Length > 0)
                {
                    documentPath = await _fileStorageService.SaveFileAsync(document, "documents");
                }

                var meeting = await _meetingService.CreateMeetingAsync(meetingDto, userId, documentPath);

                if (meeting == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Failed to create meeting"));
                }

                // TODO: Send meeting notification email

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { MeetingId = meeting.Id }, 
                    "Meeting created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating meeting");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating meeting"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMeeting(int id, [FromForm] MeetingUpdateDto meetingDto, IFormFile? document)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<object>.ErrorResponse(errors, "Validation failed"));
                }

                // Save document if provided
                string? documentPath = null;
                if (document != null && document.Length > 0)
                {
                    documentPath = await _fileStorageService.SaveFileAsync(document, "documents");
                }

                var meeting = await _meetingService.UpdateMeetingAsync(id, meetingDto, documentPath);

                if (meeting == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Meeting not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { MeetingId = meeting.Id }, 
                    "Meeting updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating meeting");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating meeting"));
            }
        }

        [HttpDelete("{id}/cancel")]
        public async Task<IActionResult> CancelMeeting(int id)
        {
            try
            {
                var result = await _meetingService.CancelMeetingAsync(id);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Meeting not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { }, 
                    "Meeting cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling meeting");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while cancelling meeting"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMeetings()
        {
            try
            {
                var meetings = await _meetingService.GetAllMeetingsAsync();
                return Ok(ApiResponse<object>.SuccessResponse(meetings, "Meetings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meetings");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving meetings"));
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserMeetings(int userId)
        {
            try
            {
                var meetings = await _meetingService.GetUserMeetingsAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(meetings, "User meetings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user meetings");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user meetings"));
            }
        }

        [HttpGet("my-meetings")]
        public async Task<IActionResult> GetMyMeetings()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("id")?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    // Fallback to hard-coded for development
                    userId = 1;
                }

                var meetings = await _meetingService.GetUserMeetingsAsync(userId);
                return Ok(ApiResponse<object>.SuccessResponse(meetings, "My meetings retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my meetings");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving my meetings"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMeetingById(int id)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst("id")?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    userId = 1;
                }

                var meeting = await _meetingService.GetMeetingByIdAsync(id);
                if (meeting == null || meeting.UserId != userId)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Meeting not found"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(meeting, "Meeting retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving meeting");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving meeting"));
            }
        }
    }
}