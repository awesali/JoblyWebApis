using JoblyWebApi.Services.Interface;

namespace JoblyWebApi.Services
{
    public class CommonService : ICommonService
    {
        private readonly IAuthService _authService;

        public CommonService(IAuthService authService)
        {
            _authService = authService;
        }

        Task<IAuthService> ICommonService.AuthService => Task.FromResult(_authService);

    }
}
