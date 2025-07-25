using joblywebapi.Models;

namespace JoblyWebApi.Interface.Naukri
{
    public interface INaukriRepository
    {
        void SaveQuestionAnswer(int userId, string question, string answer);
        string? GetAnswerByQuestion(string question);
        Task Save(AppliedJob job);
        List<AppliedJobDto> GetPagedJobsByUserId(int userId, int page, int pageSize);
        User GetById(int userId);
    }
}
