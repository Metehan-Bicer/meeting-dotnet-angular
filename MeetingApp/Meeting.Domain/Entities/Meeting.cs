using System.ComponentModel.DataAnnotations;

namespace Meeting.Domain.Entities
{
    public class MeetingEntity
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string? DocumentPath { get; set; }
        
        public bool IsCancelled { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? CancelledAt { get; set; }
        
        // Foreign key
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}