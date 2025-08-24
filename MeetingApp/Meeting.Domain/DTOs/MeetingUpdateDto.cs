using System.ComponentModel.DataAnnotations;

namespace Meeting.Domain.DTOs
{
    public class MeetingUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public bool IsCancelled { get; set; }
    }
}