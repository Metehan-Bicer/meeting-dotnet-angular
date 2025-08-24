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

                // Get user ID from JWT token (simplified for now)
                var userId = 1; // This should come from the authenticated user

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

                // Update document path if new document was uploaded
                if (documentPath != null)
                {
                    // In a real implementation, you would update the meeting with the new document path
                    // For now, we're just showing how to handle file uploads
                }

                var meeting = await _meetingService.UpdateMeetingAsync(id, meetingDto);

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
    }
}