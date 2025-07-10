namespace JoblyWebApi.Services;

using UglyToad.PdfPig;

public static class ResumeTextService
{
    public static string ExtractTextFromPdf(string path)
    {
        using var document = PdfDocument.Open(path);
        string fullText = "";
        foreach (var page in document.GetPages())
        {
            fullText += page.Text + "\n";
        }
        return fullText;
    }
}
