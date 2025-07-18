using JoblyWebApi.Data.Models;

namespace JoblyWebApi.Data
{
    public interface INaukriRepository
    {
        Task SaveJob(AppliedJob job);
        Task SaveQuestionAnswer(SaveQuestionAnswer QnA);
        Task<ResumeAnswerResult?> GetAnswerByQuestion(string question);
        Task<string?> GetResumePathAsync(int userId);
    }
}
