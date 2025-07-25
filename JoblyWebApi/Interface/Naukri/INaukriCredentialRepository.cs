namespace JoblyWebApi.Interface.Naukri
{
    public interface INaukriCredentialRepository
    {
        Task SaveNaukriCredentialsAsync(int userId, string username, string password);
        Task<NaukriCredentialDto?> GetByUserIdAsync(int userId);
    }
}
