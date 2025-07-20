namespace JoblyWebApi.Data.Models
{
    public class NaukriUser
    {
        public int UserId { get; set; }
        public string NaukriId { get; set; }
        public string NaukriPassword { get; set; }

        public string Role { get; set; }
        public string Location { get; set; }
        public string Skills { get; set; }
    }

}
