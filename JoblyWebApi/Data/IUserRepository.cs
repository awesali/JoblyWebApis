using JoblyWebApi.Data.Models;
using JoblyWebApi.Models;

namespace JoblyWebApi.Data
{
    public interface IUserRepository
    {
        Task<string> RegisterUser(UserRegister user);
        Task<bool> IsUserExists(UserLogin user);
        Task<NaukriUser> GetNaukriUser(int userId);
        Task InsertResumeQnA(SaveQuestionAnswer QaN);
        Task<string> GetAnswerByQuestion(string question);
        Task InsertAppliedJob(AppliedJob job);
        Task<UserDetails> GetUserById(int Id);
        Task<string> GetResumePathAsync(int userId);
    }
}
