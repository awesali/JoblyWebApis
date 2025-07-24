using Dapper;
using JoblyWebApi.Data.Models;
using JoblyWebApi.Models;
using System.Data;

namespace JoblyWebApi.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public UserRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<string> GetAnswerByQuestion(string question)
        {
            using var conn = _dbFactory.CreateConnection();

            var answer = await conn.QueryFirstOrDefaultAsync<string>(
                "GetAnswerByQuestion",
                new { Question = question },
                commandType: CommandType.StoredProcedure
            );

            return answer ?? string.Empty;
        }


        public async Task<NaukriUser> GetNaukriUser(int userId)
        {
            using var conn = _dbFactory.CreateConnection();

            var parameters = new { UserId = userId };

            var result = await conn.QueryFirstOrDefaultAsync<NaukriUser>(
                "GetNaukriUserById",
                parameters,
                commandType: CommandType.StoredProcedure
            );

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

        public async Task<UserDetails> GetUserById(int Id)
        {
            using var conn = _dbFactory.CreateConnection();

            var user = await conn.QueryFirstOrDefaultAsync<UserDetails>(
                "GetUserById",
                new { Id },
                commandType: CommandType.StoredProcedure
            );

            return user;
        }

        public async Task InsertAppliedJob(AppliedJob job)
        {
            using var conn = _dbFactory.CreateConnection();

            await conn.ExecuteAsync(
                "InsertAppliedJob",
                new
                {
                    job.UserId,
                    job.JobTitle,
                    job.Company,
                    job.Location,
                    job.AppliedAt
                },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task InsertResumeQnA(SaveQuestionAnswer QaN)
        {
            using var conn = _dbFactory.CreateConnection();

            await conn.ExecuteAsync(
                "InsertResumeQnA",
                new
                {
                    QaN.Question,
                    QaN.Answer,
                    QaN.UserId
                },
                commandType: CommandType.StoredProcedure
            );
        }
        
        public async Task<bool> IsUserExists(UserLogin user)
        {
            using var conn = _dbFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", user.Email);
            parameters.Add("@PasswordHash", user.Password);
            parameters.Add("@IsValid", dbType: DbType.Boolean, direction: ParameterDirection.Output);

            await conn.ExecuteAsync(
                "CheckUserExsist",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return parameters.Get<bool>("@IsValid");
        }

        public async Task<string> RegisterUser(UserRegister user)
        {
            using var conn = _dbFactory.CreateConnection();

            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "GetOrRegisterUser",
                user,
                commandType: CommandType.StoredProcedure
            );

            return result ?? "Unknown error";
        }
       
    }
}
