namespace JoblyWebApi.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using JoblyWebApi.Interface;
    using Newtonsoft.Json;
    using RestSharp;

    public interface IGroqService
    {
        Task<string> AskAsync(string question);
    }

    public sealed class GroqService : IGroqService
    {
        private readonly string _apiKey;
        private readonly int _userId;
        private string? _cachedResumeText;
        private readonly IUnitOfWork _uow;

        public GroqService(string apiKey, int userId,IUnitOfWork uow)
        {
            _apiKey = apiKey;
            _userId = userId;
            _uow = uow;
        }

        public async Task<string> AskAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return "❌ Question is empty.";

            if (string.IsNullOrWhiteSpace(_cachedResumeText))
            {
                // DI ke through aaya hua UnitOfWork
                var resumePath = _uow.Resume.GetResumePath(_userId);

                if (string.IsNullOrWhiteSpace(resumePath) || !File.Exists(resumePath))
                {
                    Console.WriteLine($"❌ Resume file not found for user {_userId}.");
                    return "Resume not found";
                }

                _cachedResumeText = _uow.Resume.ExtractTextFromPdf(resumePath);
            }


            var client = new RestClient(new RestClientOptions("https://api.groq.com/openai/v1"));
            var request = new RestRequest("chat/completions", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_apiKey}");
            request.AddHeader("Content-Type", "application/json");

            var payload = new
            {
                model = "llama3-8b-8192",
                messages = new object[]
                {
                    new { role = "system", content = "You are the person whose resume is provided. Answer each question in 2-4 words only. Do not use full sentences. Be direct and brief." },
                    new { role = "user",   content = $"Resume:\n{_cachedResumeText}\n\nQuestion: {question}" }
                },
                temperature = 0.7
            };

            request.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);

            // Retry logic without recursion
            const int maxRetries = 2;
            RestResponse? response = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                response = await client.ExecuteAsync(request);

                if ((int)response.StatusCode == 429 && attempt < maxRetries)
                {
                    Console.WriteLine("⏳ Rate limited. Retrying in 7 seconds...");
                    await Task.Delay(7000);
                    continue;
                }

                break;
            }

            if (response is null || !response.IsSuccessful)
                return $"❌ Error: {response?.StatusCode} - {response?.Content}";

            dynamic json = JsonConvert.DeserializeObject(response.Content!);
            return json?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "❌ No answer found.";
        }
       
    }
}
