using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using JoblyWebApi.Interface.Naukri;

public class NaukriCredentialRepository : INaukriCredentialRepository
{
    private readonly string _connectionString;

    public NaukriCredentialRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task SaveNaukriCredentialsAsync(int userId, string username, string password)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("usp_SaveNaukriCredential", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@Password", password);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<NaukriCredentialDto?> GetByUserIdAsync(int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("GetNaukriCredentialsByUserId", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new NaukriCredentialDto
            {
                UserId = reader["user_id"] is DBNull ? 0 : (int)reader["user_id"],
                Username = reader["username"]?.ToString() ?? string.Empty,
                Password = reader["password"]?.ToString() ?? string.Empty
            };
        }

        return null;
    }
}
