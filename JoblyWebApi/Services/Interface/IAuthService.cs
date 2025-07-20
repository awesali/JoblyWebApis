using JoblyWebApi.Data.Models;

namespace JoblyWebApi.Services.Interface
{
    public interface IAuthService
    {
        Task<string> Register(UserRegister user);
        Task<bool> CheckUser(UserLogin user);
    }
}
