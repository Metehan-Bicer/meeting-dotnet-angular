using Meeting.Application.Interfaces;
using Meeting.Domain.Entities;
using Meeting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Meeting.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MeetingDbContext _context;

        public UserRepository(MeetingDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}