using Meeting.Domain.Entities;

namespace Meeting.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<bool> UserExistsAsync(string email);
    }
}