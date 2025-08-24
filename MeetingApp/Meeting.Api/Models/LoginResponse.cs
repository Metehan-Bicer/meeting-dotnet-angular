namespace Meeting.Api.Models
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}