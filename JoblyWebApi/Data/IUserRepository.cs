using JoblyWebApi.Data.Models;

namespace JoblyWebApi.Data
{
    public interface IUserRepository
    {
        Task<string> RegisterUser(UserRegister user);
        Task<bool> IsUserExists(UserLogin user);
    }
}
