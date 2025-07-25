using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using joblywebapi.Models;
using JoblyWebApi.Interface.Naukri;

namespace JoblyWebApi.Repositories.Naukri
{
    public class NaukriRepository : INaukriRepository
    {
        private readonly string _connectionString;

        public NaukriRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void SaveQuestionAnswer(int userId, string question, string answer)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("InsertResumeQnA", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Question", question);
            cmd.Parameters.AddWithValue("@Answer", answer);
            cmd.Parameters.AddWithValue("@UserId", userId);

            cmd.ExecuteNonQuery();
        }

        public string? GetAnswerByQuestion(string question)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("GetAnswerByQuestion", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Question", question);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? reader["Answer"]?.ToString() : null;
        }

        public async Task Save(AppliedJob job)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"INSERT INTO AppliedJobs (UserId, JobTitle, Company, Location, AppliedAt)
                                   VALUES (@UserId, @JobTitle, @Company, @Location, @AppliedAt)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@UserId", job.UserId);
            cmd.Parameters.AddWithValue("@JobTitle", job.JobTitle);
            cmd.Parameters.AddWithValue("@Company", job.Company);
            cmd.Parameters.AddWithValue("@Location", job.Location);
            cmd.Parameters.AddWithValue("@AppliedAt", job.AppliedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        public List<AppliedJobDto> GetPagedJobsByUserId(int userId, int page, int pageSize)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var jobs = new List<AppliedJobDto>();

            using var cmd = new SqlCommand("GetAppliedJobsByUserIdPaged", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                jobs.Add(new AppliedJobDto
                {
                    JobTitle = reader["JobTitle"]?.ToString(),
                    Company = reader["Company"]?.ToString(),
                    Location = reader["Location"]?.ToString(),
                    AppliedAt = Convert.ToDateTime(reader["AppliedAt"])
                });
            }

            return jobs;
        }

        public User? GetById(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("dbo.Users_GetById", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Id", userId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new User
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                Name = reader["Name"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                PasswordHash = reader["PasswordHash"]?.ToString(),
                CurrentCtc = reader["CurrentCtc"]?.ToString(),
                ExpectedCtc = reader["ExpectedCtc"]?.ToString(),
                NoticePeriod = reader["NoticePeriod"]?.ToString(),
                JoinAvailability = reader["JoinAvailability"]?.ToString()
            };
        }
    }
}
