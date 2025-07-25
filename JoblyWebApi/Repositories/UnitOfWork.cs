using JoblyWebApi.Interface;
using JoblyWebApi.Interface.Naukri;
using JoblyWebApi.Repositories.Naukri;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly string _cs;

    public INaukriRepository Naukri { get; }
    public IResumeRepository Resume { get; }
    public IUserRepository Users { get; }
    public INaukriCredentialRepository NaukriCredentials { get; }

    public UnitOfWork(string connectionString)
    {
        _cs = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

        // Repos will open/close their own connections
        Users             = new UserRepository(_cs);
        Resume            = new ResumeRepository(_cs);
        Naukri            = new NaukriRepository(_cs);
        NaukriCredentials = new NaukriCredentialRepository(_cs);
    }

    public void Dispose() { /* nothing to dispose */ }
}
