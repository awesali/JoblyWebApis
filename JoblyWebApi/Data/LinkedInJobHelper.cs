namespace joblywebapi.Helpers;

public static class LinkedInJobHelper
{
    private static IConfiguration _config;

    public static void Init(IConfiguration config) => _config = config;

    public static string GetJobSearchUrl() => _config["LinkedIn:JobSearchUrl"]!;
    public static string GetChromeProfilePath() => _config["LinkedIn:ChromeProfileBasePath"]!;
    public static string GetEmail() => _config["LinkedIn:Email"]!;
    public static string GetPassword() => _config["LinkedIn:Password"]!;
    public static string GetResumePath() => _config["LinkedIn:ResumePath"]!;
    public static string GetInterviewDbConn() => _config.GetConnectionString("InterviewDb")!;
}
