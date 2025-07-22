using System.Data;
using System.Data.SqlClient;
using JoblyWebApi.Services.Interfaces;

public class NaukriCredentialRepository : INaukriCredentialRepository
{
    public async Task SaveNaukriCredentialsAsync(int userId, string username, string password)
    {
        using (SqlConnection conn = new SqlConnection(DbConnectionHelper.ConnectionString))
        using (SqlCommand cmd = new SqlCommand("usp_SaveNaukriCredential", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", password);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }

    public NaukriCredentialDto GetByUserId(int userId)
    {
        using (SqlConnection conn = new SqlConnection(DbConnectionHelper.ConnectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("GetNaukriCredentialsByUserId", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new NaukriCredentialDto
                        {
                            UserId = (int)reader["user_id"],
                            Username = reader["username"].ToString(),
                            Password = reader["password"].ToString()
                        };
                    }
                }
            }
        }

        return null;
    }
}
