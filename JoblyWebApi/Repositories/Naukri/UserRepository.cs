using System;
using System.Data;
using System.Data.SqlClient;
using joblywebapi.Models;
using JoblyWebApi.Interface;
using JoblyWebApi.Interface.Naukri;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public int Register(User user)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("dbo.Users_Register", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@Name", (object?)user.Name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@PasswordHash", (object?)user.PasswordHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CurrentCtc", (object?)user.CurrentCtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpectedCtc", (object?)user.ExpectedCtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NoticePeriod", (object?)user.NoticePeriod ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@JoinAvailability", (object?)user.JoinAvailability ?? DBNull.Value);

        var result = cmd.ExecuteScalar();
        return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
    }

    public User? GetByEmail(string email)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("GetUserByEmail", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Email", (object?)email ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                Name = reader["Name"]?.ToString() ?? "",
                Email = reader["Email"]?.ToString() ?? "",
                PasswordHash = reader["PasswordHash"]?.ToString() ?? ""
            };
        }

        return null;
    }
}
