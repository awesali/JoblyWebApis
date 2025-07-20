using JoblyWebApi.Services.Interface;

namespace JoblyWebApi.Services
{
    public class CommonService : ICommonService
    {
        private readonly IAuthService _authService;
        private readonly IApplyService _applyService;

        public CommonService(IAuthService authService,IApplyService applyService)
        {
            _authService = authService;
            _applyService = applyService;
        }

        Task<IApplyService> ICommonService.ApplyService => Task.FromResult(_applyService);
        Task<IAuthService> ICommonService.AuthService => Task.FromResult(_authService);
    }
}
