namespace JoblyWebApi.Interface.Naukri
{
    public interface IResumeRepository
    {
        void Save(int userId, string fileName);
        string? GetResumePath(int userId);
        string ExtractTextFromPdf(string path);
    }
}
