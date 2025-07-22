using System.Threading.Tasks;

public interface INaukriCredentialRepository
{
    Task SaveNaukriCredentialsAsync(int userId, string username, string password);
}
