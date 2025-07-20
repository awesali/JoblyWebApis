namespace JoblyWebApi.Services.Interface
{
    public interface ICommonService
    {
        Task<IAuthService> AuthService { get; }
        Task<IApplyService> ApplyService { get; }
    }
}
