using System.ComponentModel.DataAnnotations;

namespace Meeting.Domain.Entities
{
    public class MeetingDeleteLog
    {
        public int Id { get; set; }
        
        [Required]
        public int MeetingId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public string? DocumentPath { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public required string UserName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string UserEmail { get; set; }
        
        [Required]
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public required string DeletedBy { get; set; }
        
        [MaxLength(200)]
        public string? DeleteReason { get; set; }
        
        public bool WasCancelled { get; set; }
        
        public DateTime? CancelledAt { get; set; }
        
        public DateTime OriginalCreatedAt { get; set; }
    }
}