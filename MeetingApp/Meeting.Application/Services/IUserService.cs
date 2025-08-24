using Meeting.Domain.DTOs;
using Meeting.Domain.Entities;

namespace Meeting.Application.Services
{
    public interface IUserService
    {
        Task<User?> RegisterUserAsync(UserRegistrationDto userDto, string profileImagePath);
        Task<User?> LoginUserAsync(UserLoginDto loginDto);
        Task<User?> GetUserByIdAsync(int id);
    }
}