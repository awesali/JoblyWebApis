using System.Data.SqlClient;

public class UserRepositorys
{
    public User GetByEmail(string email)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        // ✅ Fixed: [User] instead of User
        string query = "SELECT * FROM [Users] WHERE Email = @Email";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader["Id"] != DBNull.Value ? (int)reader["Id"] : 0,
                Name = reader["Name"]?.ToString() ?? "",
                Email = reader["Email"]?.ToString() ?? "",
                PasswordHash = reader["PasswordHash"]?.ToString() ?? ""
            };
        }

        return null;
    }

    public void Register(User user)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        string query = @"INSERT INTO [Users] 
        (Name, Email, PasswordHash, CurrentCtc, ExpectedCtc, NoticePeriod, JoinAvailability) 
        VALUES 
        (@Name, @Email, @PasswordHash, @CurrentCtc, @ExpectedCtc, @NoticePeriod, @JoinAvailability)";

        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Name", user.Name ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CurrentCtc", user.CurrentCtc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpectedCtc", user.ExpectedCtc ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NoticePeriod", user.NoticePeriod ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@JoinAvailability", user.JoinAvailability ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public User GetById(int userId)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        conn.Open();

        var query = "SELECT * FROM [Users] WHERE Id = @Id";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@Id", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = (int)reader["Id"],
                Name = reader["Name"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                PasswordHash = reader["PasswordHash"]?.ToString(),
                CurrentCtc = reader["CurrentCtc"]?.ToString(),
                ExpectedCtc = reader["ExpectedCtc"]?.ToString(),
                NoticePeriod = reader["NoticePeriod"]?.ToString(),
                JoinAvailability = reader["JoinAvailability"]?.ToString()
            };
        }

        return null;
    }


}
