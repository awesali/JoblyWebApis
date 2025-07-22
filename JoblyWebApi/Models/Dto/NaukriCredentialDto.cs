using System.Text.Json.Serialization;

public class NaukriCredentialDto
{
    [JsonIgnore]
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
