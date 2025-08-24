using Meeting.Application.Interfaces;
using Meeting.Application.Services;
using Meeting.Domain.DTOs;
using Meeting.Domain.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Meeting.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> RegisterUserAsync(UserRegistrationDto userDto, string profileImagePath)
        {
            // Check if user already exists
            if (await _userRepository.UserExistsAsync(userDto.Email))
            {
                return null;
            }

            // Create new user
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                ProfileImagePath = profileImagePath
            };

            // Hash password
            user.PasswordHash = HashPassword(userDto.Password);

            // Save user
            return await _userRepository.CreateUserAsync(user);
        }

        public async Task<User?> LoginUserAsync(UserLoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
            
            if (user == null)
            {
                return null;
            }

            if (VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        private string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Combine salt and hash
            return Convert.ToBase64String(salt) + ":" + hashed;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Split the stored hash into salt and hash
                string[] parts = hashedPassword.Split(':');
                byte[] salt = Convert.FromBase64String(parts[0]);
                string hash = parts[1];

                // Hash the input password with the stored salt
                string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                // Compare the hashes
                return hash == hashedInput;
            }
            catch
            {
                return false;
            }
        }
    }
}