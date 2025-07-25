namespace JoblyWebApi.Interface.Naukri
{
    public interface IUserRepository
    {
        int Register(User user);
        User? GetByEmail(string email);
    }

}
