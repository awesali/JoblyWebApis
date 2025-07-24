using JoblyWebApi.Services;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using SeleniumExtras.WaitHelpers;
using UglyToad.PdfPig;

public class NaukriApplyEngine
{
    private readonly string _email;
    private readonly string _password;
    private readonly int _userId;
    private readonly string _apiKey;

    public NaukriApplyEngine(string email, string password, int userId, IConfiguration config)
    {
        _email = email;
        _password = password;
        _userId = userId;
        _apiKey = config["Groq:ApiKey"];
    }

    public async void Run(string role, string location, string skills)
    {
       

        
    }

   
    private string _cachedResumeText = null;
    //private async Task<string> AskGroqAsync(string question)
    //{
    //    if (_cachedResumeText == null)
    //    {
    //        if (string.IsNullOrEmpty(resumePath) || !System.IO.File.Exists(resumePath))
    //        {
    //            Console.WriteLine("❌ Resume file not found for user.");
    //            return "Resume not found";
    //        }

    //        _cachedResumeText = ExtractTextFromPdf(resumePath);
    //    }

    //    var client = new RestClient("https://api.groq.com/openai/v1/chat/completions");
    //    var request = new RestRequest("", Method.Post);
    //    request.AddHeader("Authorization", $"Bearer {_apiKey}");
    //    request.AddHeader("Content-Type", "application/json");

    //    var payload = new
    //    {
    //        model = "llama3-8b-8192",
    //        messages = new[] {
    //        new { role = "system", content = "You are the person whose resume is provided. Answer each question in 2-4 words only. Do not use full sentences. Be direct and brief." },
    //        new { role = "user", content = $"Resume:\n{_cachedResumeText}\n\nQuestion: {question}" }
    //    },
    //        temperature = 0.7
    //    };

    //    request.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);

    //    var response = await client.ExecuteAsync(request);

    //    // Retry once if rate limited
    //    if ((int)response.StatusCode == 429)
    //    {
    //        Console.WriteLine("⏳ Rate limited. Retrying in 7 seconds...");
    //        await Task.Delay(7000);
    //        return await AskGroqAsync(question);
    //    }

    //    if (!response.IsSuccessful)
    //        return $"❌ Error: {response.StatusCode} - {response.Content}";

    //    dynamic json = JsonConvert.DeserializeObject(response.Content);
    //    return json?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "❌ No answer found.";
    //}

    static string ExtractTextFromPdf(string path)
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

//}
//using JoblyWebApi.Handlers;
//using JoblyWebApi.Repositories;
//using JoblyWebApi.Services;
//using Newtonsoft.Json;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
//using SeleniumExtras.WaitHelpers;
//using JoblyWebApi.Repositories;


//public class NaukriApplyEngine
//{
//    private readonly string _email;
//    private readonly string _password;
//    private readonly int _userId;

//    public NaukriApplyEngine(string email, string password, int userId)
//    {
//        _email = email;
//        _password = password;
//        _userId = userId;
//    }

//    public async void Run(string role, string location, string skills)
//    {
//        var options = new ChromeOptions();
//        options.AddArgument("--window-size=1920,1080");

//        using var driver = new ChromeDriver(options);
//        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

//        try
//        {
//            driver.Navigate().GoToUrl("https://login.naukri.com/nLogin/Login.php");
//            ConfigManager.Delay5();
//            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usernameField"))).SendKeys(_email);
//            ConfigManager.Delay5();
//            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("passwordField"))).SendKeys(_password);
//            ConfigManager.Delay5();
//            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']"))).Click();
//            Console.WriteLine("✅ Logged in successfully");
//            ConfigManager.Delay5();

//            driver.Navigate().GoToUrl("https://www.naukri.com/mnjuser/homepage");
//            ConfigManager.Delay5();
//            var viewAll = driver.FindElements(By.CssSelector("a.view-all-link")).FirstOrDefault();
//            if (viewAll != null)
//            {
//                viewAll.Click();
//                Console.WriteLine("🔗 Clicked on 'View All'");
//                Thread.Sleep(4000);
//            }

//            string[] tabIds = { "apply", "profile", "top_candidate", "preference", "similar_jobs" };
//            int appliedCount = 0;

//            foreach (var tabId in tabIds)
//            {
//                if (appliedCount >= 5) break;

//                try
//                {
//                    var tab = driver.FindElements(By.CssSelector($"div.tab-wrapper#{tabId}")).FirstOrDefault();
//                    if (tab != null)
//                    {
//                        tab.Click();
//                        Console.WriteLine($"🟢 Tab Clicked: {tabId}");
//                        Thread.Sleep(4000);

//                        var jobs = driver.FindElements(By.CssSelector("article.jobTuple"));
//                        Console.WriteLine($"🔍 Found {jobs.Count} jobs in '{tabId}'");
//                        string originalWindow = driver.CurrentWindowHandle;

//                        for (int i = 0; i < jobs.Count; i++)
//                        {
//                            await ApplyJob(jobs[i], driver, wait);
//                            driver.SwitchTo().Window(originalWindow);
//                            Thread.Sleep(1000);
//                            if (++appliedCount >= 5) break;
//                        }
//                    }
//                }
//                catch (Exception exTab)
//                {
//                    Console.WriteLine($"❌ Tab error [{tabId}]: {exTab.Message}");
//                }
//            }

//            Console.WriteLine($"✅ Done: {appliedCount} jobs applied.");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("❌ Main Error: " + ex.Message);
//        }
//        finally
//        {
//            driver.Quit();
//        }
//    }

//    public async Task ApplyJob(IWebElement job, ChromeDriver driver, WebDriverWait wait)
//    {
//        try
//        {
//            if (!job.FindElements(By.CssSelector(".tuple-check-box i.naukicon-ot-checkbox")).Any())
//            {
//                Console.WriteLine("⚠ Skipping job without checkbox");
//                return;
//            }

//            var titleElem = job.FindElement(By.CssSelector("p.title"));
//            string title = titleElem.Text;
//            string company = job.FindElement(By.CssSelector("span.companyWrapper span[title]"))?.Text ?? "";
//            string loc = job.FindElement(By.CssSelector("li.location span"))?.Text ?? "";
//            ConfigManager.Delay5();
//            titleElem.Click();
//            Console.WriteLine($"🔗 Opened job: {title}");
//            await Task.Delay(5000);

//            string originalWindow = driver.CurrentWindowHandle;
//            wait.Until(d => d.WindowHandles.Count > 1);

//            foreach (var window in driver.WindowHandles)
//            {
//                if (window != originalWindow)
//                {
//                    driver.SwitchTo().Window(window);
//                    Console.WriteLine("🪟 Switched to job tab");
//                    break;
//                }
//            }

//            wait.Until(ExpectedConditions.ElementExists(By.Id("job_header")));

//            if (driver.PageSource.ToLower().Contains("job you are looking for is expired"))
//            {
//                Console.WriteLine("⛔ Job expired. Skipping...");
//                driver.Close();
//                driver.SwitchTo().Window(originalWindow);
//                return;
//            }

//            var applyBtn = driver.FindElements(By.Id("apply-button")).FirstOrDefault();
//            if (applyBtn == null || !applyBtn.Displayed || !applyBtn.Enabled)
//            {
//                Console.WriteLine("⚠ Apply button not available. Skipping.");
//                driver.Close();
//                driver.SwitchTo().Window(originalWindow);
//                return;
//            }

//            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applyBtn);
//            await Task.Delay(500);
//            applyBtn.Click();
//            Console.WriteLine("✅ Clicked Apply");
//            await Task.Delay(3000);

//            bool formFound = false;
//            try
//            {
//                formFound = wait.Until(d => d.FindElements(By.CssSelector("li.botLogo.chatbot_ListItem .naukLogoMsg")).Any());
//            }
//            catch { }

//            if (formFound)
//            {
//                Console.WriteLine("📝 Slide form found — answering questions...");
//                int questionIndex = 0;

//                while (true)
//                {
//                    try
//                    {
//                        // 🔁 Always get fresh list of questions (DOM changes after each answer)
//                        var questionElements = driver.FindElements(By.CssSelector("li.botItem.chatbot_ListItem .botMsg span"));

//                        if (questionElements.Count == 0)
//                        {
//                            Console.WriteLine("⚠ No questions found, exiting...");
//                            break;
//                        }

//                        if (questionIndex >= questionElements.Count)
//                        {
//                            Console.WriteLine("✅ All questions answered.");
//                            break;
//                        }

//                        var elem = questionElements[questionIndex];

//                        string question = elem.Text.Trim().ToLower();

//                        if (string.IsNullOrEmpty(question) ||
//                            question.Contains("thank you for your response") ||
//                            question.Contains("thank you for showing interest") ||
//                            question.Contains("kindly answer all") ||
//                            question.Contains("successfully apply") ||
//                            question.Contains("hi "))
//                        {
//                            Console.WriteLine("↪ Skipping irrelevant question");
//                            questionIndex++;
//                            continue;
//                        }

//                        string answer = await FormHandler.ProcessFormAnswer(_userId, driver, question);
//                        FormHandler.HandleMCQOrChips(driver, answer);

//                        questionIndex++;
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"⚠ Error while answering: {ex.Message}");
//                        break;
//                    }
//                }
//            }

//            if (!formFound)
//            {
//                await new AppliedJobRepository().Save(new AppliedJob
//                {
//                    UserId = _userId,
//                    JobTitle = title,
//                    Company = company,
//                    Location = loc,
//                    AppliedAt = DateTime.Now
//                });

//                Console.WriteLine($"💾 Job saved: {title}");
//            }

//            driver.Close();
//            driver.SwitchTo().Window(originalWindow);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("❌ Error in ApplyJob: " + ex.Message);
//        }
//    }
//}
