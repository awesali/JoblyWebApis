namespace JoblyWebApi.Data.Models
{
    public class UserRegister
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? PasswordHash { get; set; }
        public string? CurrentCtc { get; set; }
        public string? ExpectedCtc { get; set; }
        public string? NoticePeriod { get; set; }
        public string? JoinAvailability { get; set; }
    }
}
