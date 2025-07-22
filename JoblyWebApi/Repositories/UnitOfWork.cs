using JoblyWebApi.Services.Interfaces;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork()
    {
        NaukriCredentialRepository = new NaukriCredentialRepository();
    }

    public INaukriCredentialRepository NaukriCredentialRepository { get; private set; }
}
