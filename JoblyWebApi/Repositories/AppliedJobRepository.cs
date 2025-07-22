using System.Data;
using System.Data.SqlClient;
using joblywebapi.Models;


namespace JoblyWebApi.Repositories;

public class AppliedJobRepository
{
    public async Task Save(AppliedJob job)
    {
        using var conn = new SqlConnection(DbConnectionHelper.ConnectionString);
        await conn.OpenAsync(); // ✅ async open

        string query = @"INSERT INTO AppliedJobs (UserId, JobTitle, Company, Location, AppliedAt)
                     VALUES (@UserId, @JobTitle, @Company, @Location, @AppliedAt)";
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@UserId", job.UserId);
        cmd.Parameters.AddWithValue("@JobTitle", job.JobTitle);
        cmd.Parameters.AddWithValue("@Company", job.Company);
        cmd.Parameters.AddWithValue("@Location", job.Location);
        cmd.Parameters.AddWithValue("@AppliedAt", job.AppliedAt);

        await cmd.ExecuteNonQueryAsync(); // ✅ async execute
    }


    public List<AppliedJobDto> GetPagedJobsByUserId(int userId, int page, int pageSize)
    {
        var jobs = new List<AppliedJobDto>();

        using (SqlConnection conn = new SqlConnection(DbConnectionHelper.ConnectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("GetAppliedJobsByUserIdPaged", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Page", page);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        jobs.Add(new AppliedJobDto
                        {
                            JobTitle = reader["JobTitle"].ToString(),
                            Company = reader["Company"].ToString(),
                            Location = reader["Location"].ToString(),
                            AppliedAt = Convert.ToDateTime(reader["AppliedAt"])
                        });
                    }
                }
            }
        }

        return jobs;
    }
}
