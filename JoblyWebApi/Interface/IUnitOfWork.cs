using JoblyWebApi.Interface.Naukri;

namespace JoblyWebApi.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        INaukriRepository Naukri { get; }
        IResumeRepository Resume { get; }
        IUserRepository Users { get; }
        INaukriCredentialRepository NaukriCredentials { get; }
    }
}
