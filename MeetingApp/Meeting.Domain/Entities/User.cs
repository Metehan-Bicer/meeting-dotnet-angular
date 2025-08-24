using System.ComponentModel.DataAnnotations;

namespace Meeting.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        public string? ProfileImagePath { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}