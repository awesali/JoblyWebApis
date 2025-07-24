namespace JoblyWebApi.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal CurrentCtc { get; set; }
        public decimal ExpectedCtc { get; set; }
        public int NoticePeriod { get; set; }
        public DateTime JoinAvailability { get; set; }
        public string NaukriId { get; set; }
    }
}
