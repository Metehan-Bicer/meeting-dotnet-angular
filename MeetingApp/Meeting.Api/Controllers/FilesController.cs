using Meeting.Api.Models;
using Meeting.Application.Services;
using Meeting.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Meeting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly ISecureFileService _secureFileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(ISecureFileService secureFileService, ILogger<FilesController> logger)
        {
            _secureFileService = secureFileService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string fileType, [FromForm] int? relatedEntityId = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("No file provided"));
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                }

                // Validate file type parameter
                if (fileType != FileTypes.ProfileImage && fileType != FileTypes.MeetingDocument)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid file type"));
                }

                var fileMetadata = await _secureFileService.SaveFileAsync(file, userId.Value, fileType, relatedEntityId);

                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    fileId = fileMetadata.Id,
                    originalFileName = fileMetadata.OriginalFileName,
                    storedFileName = fileMetadata.StoredFileName,
                    fileSize = fileMetadata.FileSize,
                    uploadedAt = fileMetadata.UploadedAt
                }, "File uploaded successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while uploading the file"));
            }
        }

        [HttpGet("{fileId:int}")]
        public async Task<IActionResult> GetFile(int fileId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized();
                }

                var result = await _secureFileService.GetFileAsync(fileId, userId.Value);
                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("File not found or access denied"));
                }

                var (fileStream, fileInfo) = result.Value;

                return File(fileStream, fileInfo.ContentType, fileInfo.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file {FileId}", fileId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the file"));
            }
        }

        [HttpGet("by-name/{storedFileName}")]
        public async Task<IActionResult> GetFileByName(string storedFileName)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized();
                }

                var result = await _secureFileService.GetFileByNameAsync(storedFileName, userId.Value);
                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("File not found or access denied"));
                }

                var (fileStream, fileInfo) = result.Value;

                return File(fileStream, fileInfo.ContentType, fileInfo.OriginalFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file by name {FileName}", storedFileName);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving the file"));
            }
        }

        [HttpDelete("{fileId:int}")]
        public async Task<IActionResult> DeleteFile(int fileId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                }

                var success = await _secureFileService.DeleteFileAsync(fileId, userId.Value);
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("File not found or access denied"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "File deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", fileId);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting the file"));
            }
        }

        [HttpGet("my-files")]
        public async Task<IActionResult> GetMyFiles([FromQuery] string? fileType = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                }

                var files = await _secureFileService.GetUserFilesAsync(userId.Value, fileType);

                var fileList = files.Select(f => new
                {
                    id = f.Id,
                    originalFileName = f.OriginalFileName,
                    storedFileName = f.StoredFileName,
                    fileSize = f.FileSize,
                    fileType = f.FileType,
                    contentType = f.ContentType,
                    uploadedAt = f.UploadedAt,
                    lastAccessedAt = f.LastAccessedAt,
                    relatedEntityId = f.RelatedEntityId
                }).ToList();

                return Ok(ApiResponse<object>.SuccessResponse(fileList, "Files retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user files");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving files"));
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateFile(IFormFile file, [FromForm] string fileType)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("No file provided"));
                }

                var isValid = await _secureFileService.ValidateFileAsync(file, fileType);

                return Ok(ApiResponse<object>.SuccessResponse(new { isValid }, 
                    isValid ? "File is valid" : "File validation failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while validating the file"));
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}