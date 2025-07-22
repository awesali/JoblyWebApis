namespace JoblyWebApi.Services;

using Newtonsoft.Json;
using RestSharp;

public static class GroqService
{
    public static async Task<string> AskGroqAsync(string resumeText, string question)
    {
        var client = new RestClient("https://api.groq.com/openai/v1/chat/completions");
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Authorization", $"Bearer");
        request.AddHeader("Content-Type", "application/json");

        var payload = new
        {
            model = "llama3-8b-8192",
            messages = new[] {
                new { role = "system", content = "You are the person whose resume is provided. Answer each question in 2-4 words only. Do not use full sentences. Be direct and brief." },
                new { role = "user", content = $"Resume:\n{resumeText}\n\nQuestion: {question}" }
            },
            temperature = 0.7
        };

        request.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);
        var response = await client.ExecuteAsync(request);

        if ((int)response.StatusCode == 429)
        {
            Console.WriteLine("⏳ Rate limited. Retrying in 7 seconds...");
            await Task.Delay(7000);
            return await AskGroqAsync(resumeText, question);
        }

        if (!response.IsSuccessful)
            return $"❌ Error: {response.StatusCode} - {response.Content}";

        dynamic json = JsonConvert.DeserializeObject(response.Content);
        return json?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "❌ No answer found.";
    }
}
