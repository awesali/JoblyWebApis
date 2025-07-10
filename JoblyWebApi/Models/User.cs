
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public string? CurrentCtc { get; set; }
    public string? ExpectedCtc { get; set; }
    public string? NoticePeriod { get; set; }
    public string? JoinAvailability { get; set; }
    internal static string? FindFirst(string nameIdentifier)
    {
        throw new NotImplementedException();
    }
}
