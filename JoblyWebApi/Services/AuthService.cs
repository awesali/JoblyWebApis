using JoblyWebApi.Data;
using JoblyWebApi.Data.Models;
using JoblyWebApi.Services.Interface;

namespace JoblyWebApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public Task<string> Register(UserRegister user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            return _userRepository.RegisterUser(user);
        }
    }
}
