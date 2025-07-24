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

       

        public async Task SaveJob(AppliedJobNaukri job)
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
