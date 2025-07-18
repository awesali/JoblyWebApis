namespace JoblyWebApi.Services.Interface
{
    public interface ICommonService
    {
        Task<IAuthService> AuthService { get; }
    }
}
