using Dapper;
using JoblyWebApi.Data.Models;
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

        public async Task<string> GetUser(UserLogin user)
        {
            using var conn = _dbFactory.CreateConnection();

            var result = await conn.QueryFirstOrDefaultAsync<string>(
                "GetOrRegisterUser",
                user,
                commandType: CommandType.StoredProcedure
            );

            return result ?? "Unknown error";
        }

        public Task<bool> IsUserExists(UserLogin user)
        {
            throw new NotImplementedException();
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
