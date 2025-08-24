using Meeting.Api.Models;
using Meeting.Api.Services;
using Meeting.Application.Services;
using Meeting.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Meeting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService, 
            IJwtService jwtService, 
            IFileStorageService fileStorageService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _jwtService = jwtService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegistrationDto userDto, IFormFile? profileImage)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<object>.ErrorResponse(errors, "Validation failed"));
                }

                // Save profile image if provided
                string? profileImagePath = null;
                if (profileImage != null && profileImage.Length > 0)
                {
                    profileImagePath = await _fileStorageService.SaveFileAsync(profileImage, "profiles");
                }

                var user = await _userService.RegisterUserAsync(userDto, profileImagePath ?? string.Empty);

                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("User already exists"));
                }

                // TODO: Send welcome email

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { UserId = user.Id }, 
                    "User registered successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while registering user"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                    return BadRequest(ApiResponse<object>.ErrorResponse(errors, "Validation failed"));
                }

                var user = await _userService.LoginUserAsync(loginDto);

                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid credentials"));
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Email);

                var response = new LoginResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                };

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(
                    response, 
                    "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while logging in"));
            }
        }
    }
}