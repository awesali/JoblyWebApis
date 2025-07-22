namespace JoblyWebApi.Services.Interfaces
{
    public interface INaukriLoginService
    {
        Task<bool> TryLoginAsync(string username, string password);
    }
}
