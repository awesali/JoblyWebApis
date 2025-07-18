using Dapper;
using JoblyWebApi.Data.Models;
using System.Data;

namespace JoblyWebApi.Data.Repository
{
    public class NaukriRepository : INaukriRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public NaukriRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<ResumeAnswerResult?> GetAnswerByQuestion(string question)
        {
            using var conn = _dbFactory.CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<ResumeAnswerResult>(
                "GetAnswerByQuestion",
                new { Question = question }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<string?> GetResumePathAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();

            var fileName = await conn.QueryFirstOrDefaultAsync<string>(
                "GetLatestResumeFileName",
                new { UserId = userId },
                commandType: CommandType.StoredProcedure);

            if (string.IsNullOrEmpty(fileName)) return null;

            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "resumes", userId.ToString(), fileName);
        }

        public async Task SaveJob(AppliedJob job)
        {
            using var conn = _dbFactory.CreateConnection();

            await conn.ExecuteAsync("SaveAppliedJob",  
                job,                  
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task SaveQuestionAnswer(SaveQuestionAnswer QnA)
        {
            using var conn = _dbFactory.CreateConnection();

            await conn.ExecuteAsync("InsertResumeQnA",
                QnA,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
