namespace JoblyWebApi.Data.Models
{
    public class AppliedJob
    {
        public int UserId { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}
