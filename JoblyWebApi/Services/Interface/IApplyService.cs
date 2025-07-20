using JoblyWebApi.Data.Models;

namespace JoblyWebApi.Services.Interface
{
    public interface IApplyService 
    {
        Task<NaukriUser> GetNaukriUser(int userId);
        Task<bool> ExecuteApply(NaukriUser user);
    }
}
