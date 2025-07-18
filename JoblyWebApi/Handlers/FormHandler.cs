namespace JoblyWebApi.Handlers;
using JoblyWebApi.Services;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

public class FormHandler
{
    private static string _cachedResumeText = null;

    public static async Task<string> ProcessFormAnswer(int userId, IWebDriver driver, string question)
    {
        Console.WriteLine($"❓ Question: {question}");

        string answer = ResolveAnswer(userId, question);
        string CheckAnswer = "";

        if (string.IsNullOrEmpty(answer))
        {
            CheckAnswer = ResumeRepository.GetAnswerByQuestion(question);

            if (string.IsNullOrEmpty(_cachedResumeText))
                _cachedResumeText = ResumeTextService.ExtractTextFromPdf("C:/Users/USER/Downloads/AwesResume.pdf");

            answer = string.IsNullOrEmpty(CheckAnswer)
                ? await GroqService.AskGroqAsync(_cachedResumeText, question)
                : CheckAnswer;
        }

        string source = string.IsNullOrEmpty(CheckAnswer) ? "API" : "DB";
        Console.WriteLine($"✅ Answer: {answer} (from {source})");

        ResumeRepository.SaveQuestionAnswer(userId, question, answer);

        var inputBox = driver.FindElement(By.CssSelector("div.footerInputBoxWrapper div.textArea[contenteditable='true']"));

        string script = @"
            const inputBox = arguments[0];
            const text = arguments[1];
            inputBox.innerText = text;

            const event = new Event('input', { bubbles: true });
            inputBox.dispatchEvent(event);

            const evt = new Event('change', { bubbles: true });
            inputBox.dispatchEvent(evt);
        ";

        ((IJavaScriptExecutor)driver).ExecuteScript(script, inputBox, answer);
        await Task.Delay(5000);

        return answer;
    }

    public static void HandleMCQOrChips(IWebDriver driver, string answer)
    {
        // ✅ Radio buttons
        var radios = driver.FindElements(By.CssSelector("div.ssrc__radio-btn-container input.ssrc__radio"));
        bool selected = false;

        foreach (var radio in radios)
        {
            var value = radio.GetAttribute("value")?.Trim().ToLower();
            if (!string.IsNullOrEmpty(value) && answer.ToLower().Contains(value))
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(@"
                    arguments[0].checked = true;
                    arguments[0].dispatchEvent(new Event('change', { bubbles: true }));
                ", radio);
                Console.WriteLine($"✅ MCQ Selected: {value}");
                selected = true;
                break;
            }
        }

        if (!selected && radios.Count > 0)
        {
            ((IJavaScriptExecutor)driver).ExecuteScript(@"
                arguments[0].checked = true;
                arguments[0].dispatchEvent(new Event('change', { bubbles: true }));
            ", radios[0]);
            Console.WriteLine("⚠️ No match. Defaulted to first MCQ option.");
        }

        // ✅ Chips
        if (!selected)
        {
            var chipOptions = driver.FindElements(By.CssSelector("div.chatbot_Chip.chipInRow.chipItem span"));
            foreach (var chip in chipOptions)
            {
                string chipText = chip.Text.Trim().ToLower();
                if (chipText.Contains(answer.Trim().ToLower()))
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", chip);
                    Console.WriteLine($"✅ Selected CHIP option: {chipText}");
                    selected = true;
                    break;
                }
            }

            if (!selected && chipOptions.Count > 0)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", chipOptions[0]);
                Console.WriteLine("⚠️ No match in CHIP. Defaulted to first chip option.");
            }
        }

        var saveButton = driver.FindElement(By.CssSelector("div.sendMsg"));
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", saveButton);
        Console.WriteLine("💾 Clicked 'Save' button using JavaScript");
    }

    private static string ResolveAnswer(int userId, string question)
    {
        string q = question.ToLower();
        var user = new UserRepository().GetById(userId);

        if (q.Contains("current ctc")) return user.CurrentCtc ?? "";
        if (q.Contains("expected ctc")) return user.ExpectedCtc ?? "";
        if (q.Contains("notice period")) return user.NoticePeriod ?? "";
        if (q.Contains("how soon") || q.Contains("when can you join")) return user.JoinAvailability ?? "";

        return "";
    }
}
