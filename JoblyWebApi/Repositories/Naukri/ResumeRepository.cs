using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using JoblyWebApi.Interface.Naukri;
using UglyToad.PdfPig;

public class ResumeRepository : IResumeRepository
{
    private readonly string _connectionString;

    public ResumeRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public void Save(int userId, string fileName)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("dbo.UserResume_Save", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FileName", (object?)fileName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UploadedAt", DateTime.UtcNow);

        cmd.ExecuteNonQuery();
    }

    public string? GetResumePath(int userId)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        using var cmd = new SqlCommand("dbo.UserResume_GetLatestFileName", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        var fileName = cmd.ExecuteScalar() as string;
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes", userId.ToString());
        return Path.Combine(basePath, fileName);
    }

    public string ExtractTextFromPdf(string path)
    {
        using var document = PdfDocument.Open(path);
        var fullText = "";
        foreach (var page in document.GetPages())
        {
            fullText += page.Text + "\n";
        }
        return fullText;
    }
}
