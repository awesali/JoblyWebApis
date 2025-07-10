//using UglyToad.PdfPig;
//using JoblyWebApi.Repositories;
//using Newtonsoft.Json;
//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
//using RestSharp;
//using SeleniumExtras.WaitHelpers;
//using System.Data.SqlClient;

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
//        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));

//        try
//        {
//            // Login
//            driver.Navigate().GoToUrl("https://login.naukri.com/nLogin/Login.php");
//            Thread.Sleep(3000);

//            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usernameField"))).SendKeys(_email);
//            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("passwordField"))).SendKeys(_password);
//            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']"))).Click();
//            Console.WriteLine("✅ Logged in successfully");
//            Thread.Sleep(5000);

//            // Go to Homepage -> View All Jobs
//            driver.Navigate().GoToUrl("https://www.naukri.com/mnjuser/homepage");
//            Thread.Sleep(4000);
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
//                            var job = jobs[i];

//                            await ApplyJob(job, driver, wait);

//                            // After ApplyJob completes, you should be back to original window
//                            driver.SwitchTo().Window(originalWindow);
//                            Thread.Sleep(1000);  // small pause if needed
//                            if (i == 5) break;
//                        }

//                        //await Task.WhenAll(tasks);
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
//            bool hasCheckbox = job.FindElements(By.CssSelector(".tuple-check-box i.naukicon-ot-checkbox")).Any();
//            if (!hasCheckbox)
//            {
//                Console.WriteLine("⚠️ Skipping job without checkbox");
//                return;
//            }

//            var titleElem = job.FindElement(By.CssSelector("p.title"));
//            string title = titleElem.Text;
//            string company = job.FindElement(By.CssSelector("span.companyWrapper span[title]"))?.Text ?? "";
//            string loc = job.FindElement(By.CssSelector("li.location span"))?.Text ?? "";

//            titleElem.Click();
//            Console.WriteLine($"🔗 Opened job: {title}");
//            await Task.Delay(5000);

//            string originalWindow = driver.CurrentWindowHandle;
//            wait.Until(driver => driver.WindowHandles.Count > 1);

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

//            var applyButtons = driver.FindElements(By.Id("apply-button"));
//            if (applyButtons.Count == 0)
//            {
//                Console.WriteLine("⚠️ Apply button not found, skipping.");
//                driver.Close();
//                driver.SwitchTo().Window(originalWindow);
//                return;
//            }

//            var applyBtn = applyButtons[0];

//            if (!applyBtn.Displayed || !applyBtn.Enabled)
//            {
//                Console.WriteLine("⚠️ Apply button is not clickable, skipping.");
//                driver.Close();
//                driver.SwitchTo().Window(originalWindow);
//                return;
//            }

//            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applyBtn);
//            await Task.Delay(500);

//            applyBtn.Click();
//            Console.WriteLine("✅ Clicked Apply");

//            await Task.Delay(3000);

//            // Scenario 1: Check if success message appears
//            bool successMessageFound = false;
//            try
//            {
//                //successMessageFound = wait.Until(driver =>
//                //    driver.PageSource.Contains("applied successfully") ||
//                //    driver.FindElements(By.XPath("//*[contains(text(), 'applied successfully')]")).Any()
//                //);

//                //if (successMessageFound)
//                //{
//                //    Console.WriteLine("🎉 Job applied successfully message found.");
//                //}
//            }
//            catch { /* Ignored if not found */ }

//            // Scenario 2: Check for slide form (naukLogoMsg)
//            bool formFound = false;
//            try
//            {
//                formFound = wait.Until(driver =>
//                    driver.FindElements(By.CssSelector("li.botLogo.chatbot_ListItem .naukLogoMsg")).Any()
//                );

//                if (formFound)
//                {
//                    Console.WriteLine("📝 Slide form found — manual input required.");

//                    int questionIndex = 0;
//                    while (true)
//                    {
//                        try
//                        {
//                            var questionElements = driver.FindElements(By.CssSelector("li.botItem.chatbot_ListItem .botMsg span"));
//                            if (questionElements.Count <= questionIndex)
//                            {
//                                Console.WriteLine("✅ No more questions.");
//                                break;
//                            }

//                            var elem = questionElements[questionIndex];
//                            string question = elem.Text.Trim();
//                            string questionLower = question.ToLower();

//                            // ✅ Stop if final message received
//                            if (questionLower.Contains("thank you for your response"))
//                            {
//                                Console.WriteLine("🛑 Final message received: Stopping question-answer loop.");
//                                break;
//                            }

//                            // ⛔ Skip greeting/instruction messages
//                            string[] skipPhrases = {
//            "thank you for showing interest",
//            "kindly answer all the recruiter's questions",
//            "successfully apply for the job",
//            "hi awes ali"
//        };

//                            if (!string.IsNullOrEmpty(question) &&
//                                skipPhrases.Any(p => questionLower.Contains(p)))
//                            {
//                                Console.WriteLine("↪️ Skipping greeting/instruction message");
//                                questionIndex++;
//                                continue;
//                            }

//                            // ✅ Process valid questions
//                            if (!string.IsNullOrEmpty(question) &&
//                                (question.EndsWith("?")))
//                            {
//                                Console.WriteLine($"❓ Question: {question}");

//                                var answer = await AskGroqAsync(question);
//                                Console.WriteLine($"📝 Answered: {answer}");

//                                var inputBox = driver.FindElement(By.CssSelector("div.footerInputBoxWrapper div.textArea[contenteditable='true']"));

//                                string script = @"
//                const inputBox = arguments[0];
//                const text = arguments[1];
//                inputBox.innerText = text;

//                const event = new Event('input', { bubbles: true });
//                inputBox.dispatchEvent(event);

//                const evt = new Event('change', { bubbles: true });
//                inputBox.dispatchEvent(evt);
//            ";
//                                ((IJavaScriptExecutor)driver).ExecuteScript(script, inputBox, answer);
//                                await Task.Delay(1000);

//                                var optionLabels = driver.FindElements(By.CssSelector("div.ssrc__radio-btn-container label.ssrc__label"));

//                                bool matched = false;

//                                foreach (var label in optionLabels)
//                                {
//                                    string optionText = label.Text.Trim().ToLower();
//                                    if (optionText.Contains(answer.Trim().ToLower()))
//                                    {
//                                        label.Click();  // Label pe click karna radio select kar dega
//                                        Console.WriteLine($"✅ Selected MCQ option: {optionText}");
//                                        matched = true;
//                                        break;
//                                    }
//                                }

//                                // Step 3: Agar koi match nahi mila toh fallback
//                                if (!matched && optionLabels.Count > 0)
//                                {
//                                    optionLabels[0].Click();  // Default: first option
//                                    Console.WriteLine("⚠️ No exact match. Clicked first option as fallback.");
//                                }


//                                var saveButton = driver.FindElement(By.CssSelector("div.sendMsg"));
//                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", saveButton);
//                                Console.WriteLine("💾 Clicked 'Save' button using JavaScript");

//                                await Task.Delay(3000);
//                            }

//                            questionIndex++;
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine($"⚠️ Error while answering question: {ex.Message}");
//                            break;
//                        }
//                    }



//                }

//            }
//            catch { /* Ignored if not found */ }

//            // Act based on what we found
//            if (successMessageFound)
//            {
//                new AppliedJobRepository().Save(new AppliedJob
//                {
//                    UserId = _userId,
//                    JobTitle = title,
//                    Company = company,
//                    Location = loc,
//                    AppliedAt = DateTime.Now
//                });

//                Console.WriteLine($"💾 Job saved as applied: {title}");
//            }
//            else if (formFound)
//            {
//                Console.WriteLine("⚠️ Job requires manual form submission. Skipping automated save.");
//            }
//            else
//            {
//                Console.WriteLine("❌ Neither success message nor form found. Possibly error page.");
//            }

//            driver.Close();
//            driver.SwitchTo().Window(originalWindow);
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine("❌ Error in ApplyJob: " + ex.Message);
//        }
//    }
//    private static string _cachedResumeText = null;
//    static async Task<string> AskGroqAsync(string question)
//    {
//        if (_cachedResumeText == null)
//            _cachedResumeText = ExtractTextFromPdf("C:/Users/USER/Downloads/AwesResume.pdf");

//        var client = new RestClient("https://api.groq.com/openai/v1/chat/completions");
//        var request = new RestRequest("", Method.Post);
//        request.AddHeader("Content-Type", "application/json");

//        var payload = new
//        {
//            model = "llama3-8b-8192",
//            messages = new[] {
//            new { role = "system", content = "You are the person whose resume is provided. Answer each question in 2-4 words only. Do not use full sentences. Be direct and brief." },
//            new { role = "user", content = $"Resume:\n{_cachedResumeText}\n\nQuestion: {question}" }
//        },
//            temperature = 0.7
//        };

//        request.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);

//        var response = await client.ExecuteAsync(request);

//        // Retry once if rate limited
//        if ((int)response.StatusCode == 429)
//        {
//            Console.WriteLine("⏳ Rate limited. Retrying in 7 seconds...");
//            await Task.Delay(7000);
//            return await AskGroqAsync(question);
//        }

//        if (!response.IsSuccessful)
//            return $"❌ Error: {response.StatusCode} - {response.Content}";

//        dynamic json = JsonConvert.DeserializeObject(response.Content);
//        return json?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "❌ No answer found.";
//    }

//    static string ExtractTextFromPdf(string path)
//    {
//        using var document = PdfDocument.Open(path);
//        string fullText = "";
//        foreach (var page in document.GetPages())
//        {
//            fullText += page.Text + "\n";
//        }
//        return fullText;
//    }

//}

//Tanzeel Bhai ===>
using JoblyWebApi.Repositories;
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
        var options = new ChromeOptions();
        options.AddArgument("--window-size=1920,1080");

        using var driver = new ChromeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

        try
        {
            // Login
            driver.Navigate().GoToUrl("https://login.naukri.com/nLogin/Login.php");
            ConfigManager.Delay5();

            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("usernameField"))).SendKeys(_email);
            ConfigManager.Delay5();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("passwordField"))).SendKeys(_password);
            ConfigManager.Delay5();
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[type='submit']"))).Click();
            Console.WriteLine("✅ Logged in successfully");
            ConfigManager.Delay5();

            // Go to Homepage -> View All Jobs
            driver.Navigate().GoToUrl("https://www.naukri.com/mnjuser/homepage");
            ConfigManager.Delay5();
            var viewAll = driver.FindElements(By.CssSelector("a.view-all-link")).FirstOrDefault();
            if (viewAll != null)
            {
                viewAll.Click();
                Console.WriteLine("🔗 Clicked on 'View All'");
                Thread.Sleep(4000);
            }

            string[] tabIds = { "apply", "profile", "top_candidate", "preference", "similar_jobs" };
            int appliedCount = 0;

            foreach (var tabId in tabIds)
            {
                if (appliedCount >= 5) break;



                try
                {
                    var tab = driver.FindElements(By.CssSelector($"div.tab-wrapper#{tabId}")).FirstOrDefault();
                    if (tab != null)
                    {
                        tab.Click();
                        Console.WriteLine($"🟢 Tab Clicked: {tabId}");
                        Thread.Sleep(4000);

                        var jobs = driver.FindElements(By.CssSelector("article.jobTuple"));
                        Console.WriteLine($"🔍 Found {jobs.Count} jobs in '{tabId}'");
                        string originalWindow = driver.CurrentWindowHandle;

                        for (int i = 0; i < jobs.Count; i++)
                        {
                            var job = jobs[i];

                            await ApplyJob(job, driver, wait);

                            // After ApplyJob completes, you should be back to original window
                            driver.SwitchTo().Window(originalWindow);
                            Thread.Sleep(1000);  // small pause if needed
                            if (i == 5) break;
                        }

                        //await Task.WhenAll(tasks);
                    }
                }
                catch (Exception exTab)
                {
                    Console.WriteLine($"❌ Tab error [{tabId}]: {exTab.Message}");
                }
            }

            Console.WriteLine($"✅ Done: {appliedCount} jobs applied.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Main Error: " + ex.Message);
        }
        finally
        {
            driver.Quit();
        }
    }

    public async Task ApplyJob(IWebElement job, ChromeDriver driver, WebDriverWait wait)
    {
        try
        {
            bool hasCheckbox = job.FindElements(By.CssSelector(".tuple-check-box i.naukicon-ot-checkbox")).Any();
            if (!hasCheckbox)
            {
                Console.WriteLine("⚠ Skipping job without checkbox");
                return;
            }

            var titleElem = job.FindElement(By.CssSelector("p.title"));
            string title = titleElem.Text;
            string company = job.FindElement(By.CssSelector("span.companyWrapper span[title]"))?.Text ?? "";
            string loc = job.FindElement(By.CssSelector("li.location span"))?.Text ?? "";
            ConfigManager.Delay5();
            titleElem.Click();
            Console.WriteLine($"🔗 Opened job: {title}");
            await Task.Delay(5000);

            string originalWindow = driver.CurrentWindowHandle;
            wait.Until(driver => driver.WindowHandles.Count > 1);

            foreach (var window in driver.WindowHandles)
            {
                if (window != originalWindow)
                {
                    ConfigManager.Delay5();
                    driver.SwitchTo().Window(window);
                    Console.WriteLine("🪟 Switched to job tab");
                    break;
                }
            }

            wait.Until(ExpectedConditions.ElementExists(By.Id("job_header")));
            // ✅ Check if job is expired
            bool isJobExpired = driver.PageSource.ToLower().Contains("job you are looking for is expired");

            if (isJobExpired)
            {
                Console.WriteLine("⛔ Job expired. Skipping...");
                driver.Close();
                driver.SwitchTo().Window(originalWindow);
                return;
            }

            var applyButtons = driver.FindElements(By.Id("apply-button"));
            if (applyButtons.Count == 0)
            {
                Console.WriteLine("⚠ Apply button not found, skipping.");
                ConfigManager.Delay5();
                driver.Close();
                ConfigManager.Delay5();
                driver.SwitchTo().Window(originalWindow);
                return;
            }

            var applyBtn = applyButtons[0];

            if (!applyBtn.Displayed || !applyBtn.Enabled)
            {
                Console.WriteLine("⚠ Apply button is not clickable, skipping.");
                ConfigManager.Delay5();
                driver.Close();
                ConfigManager.Delay5();
                driver.SwitchTo().Window(originalWindow);
                return;
            }

            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", applyBtn);
            await Task.Delay(500);

            applyBtn.Click();
            Console.WriteLine("✅ Clicked Apply");

            await Task.Delay(3000);

            // Scenario 1: Check if success message appears
            bool successMessageFound = false;
            try
            {
                //successMessageFound = wait.Until(driver =>
                //    driver.PageSource.Contains("applied successfully") ||
                //    driver.FindElements(By.XPath("//*[contains(text(), 'applied successfully')]")).Any()
                //);

                //if (successMessageFound)
                //{
                //    Console.WriteLine("🎉 Job applied successfully message found.");
                //}
            }
            catch { /* Ignored if not found */ }

            // Scenario 2: Check for slide form (naukLogoMsg)
            bool formFound = false;
            try
            {
                formFound = wait.Until(driver =>
                    driver.FindElements(By.CssSelector("li.botLogo.chatbot_ListItem .naukLogoMsg")).Any()
                );

                if (formFound)
                {
                    Console.WriteLine("📝 Slide form found — manual input required.");

                    int questionIndex = 0;
                    while (true)
                    {
                        try
                        {
                            var questionElements = driver.FindElements(By.CssSelector("li.botItem.chatbot_ListItem .botMsg span"));
                            if (questionElements.Count <= questionIndex)
                            {
                                Console.WriteLine("✅ No more questions.");
                                break;
                            }

                            var elem = questionElements[questionIndex];
                            string question = elem.Text.Trim();
                            string questionLower = question.ToLower();

                            // ✅ Stop if final message received
                            if (questionLower.Contains("thank you for your response"))
                            {
                                Console.WriteLine("🛑 Final message received: Stopping question-answer loop.");
                                break;
                            }

                            // ⛔ Skip greeting/instruction messages
                            string[] skipPhrases = {
                                "thank you for showing interest",
                                "kindly answer all the recruiter's questions",
                                "successfully apply for the job",
                                "hi Tanzeel"
                                    };

                            if (!string.IsNullOrEmpty(question) &&
                                skipPhrases.Any(p => questionLower.Contains(p)))
                            {
                                Console.WriteLine("↪ Skipping greeting/instruction message");
                                questionIndex++;
                                continue;
                            }

                            // ✅ Process valid questions
                            if (!string.IsNullOrEmpty(question))
                            {
                                Console.WriteLine($"❓ Question: {question}");

                                string answer = ResolveAnswer(question);
                                string CheckAnswer = "";

                                if (string.IsNullOrEmpty(answer))
                                {
                                    CheckAnswer = ResumeRepository.GetAnswerByQuestion(question);
                                    answer = string.IsNullOrEmpty(CheckAnswer)
                                        ? await AskGroqAsync(question)
                                        : CheckAnswer;
                                }

                                var ConsoleValue = string.IsNullOrEmpty(CheckAnswer)
                                    ? $"answer : {answer} is from API"
                                    : $"answer : {answer} is from DB";
                                Console.WriteLine(ConsoleValue);
                                Console.WriteLine($"📝 Answered: {answer}");

                                ResumeRepository.SaveQuestionAnswer(_userId, question, answer);

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
                                        selected = true;
                                    }
                                }

                                var saveButton = driver.FindElement(By.CssSelector("div.sendMsg"));
                                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", saveButton);
                                Console.WriteLine("💾 Clicked 'Save' button using JavaScript");

                                await Task.Delay(5000);
                            }


                            questionIndex++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠ Error while answering question: {ex.Message}");
                            break;
                        }
                    }
                }

            }
            catch { /* Ignored if not found */ }

            // Act based on what we found
            if (successMessageFound)
            {
                await new AppliedJobRepository().Save(new AppliedJob
                {
                    UserId = _userId,
                    JobTitle = title,
                    Company = company,
                    Location = loc,
                    AppliedAt = DateTime.Now
                });

                Console.WriteLine($"💾 Job saved as applied: {title}");
            }
            else if (formFound)
            {
                Console.WriteLine("⚠ Job requires manual form submission. Skipping automated save.");
            }
            else
            {
                Console.WriteLine("❌ Neither success message nor form found. Possibly error page.");
            }
            //  ConfigManager.Delay5();
            driver.Close();
            driver.SwitchTo().Window(originalWindow);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error in ApplyJob: " + ex.Message);
        }
    }
    private string _cachedResumeText = null;
    private async Task<string> AskGroqAsync(string question)
    {
        if (_cachedResumeText == null)
        {
            string resumePath = new ResumeRepository().GetResumePath(_userId);
            if (string.IsNullOrEmpty(resumePath) || !System.IO.File.Exists(resumePath))
            {
                Console.WriteLine("❌ Resume file not found for user.");
                return "Resume not found";
            }

            _cachedResumeText = ExtractTextFromPdf(resumePath);
        }

        var client = new RestClient("https://api.groq.com/openai/v1/chat/completions");
        var request = new RestRequest("", Method.Post);
        request.AddHeader("Authorization", $"Bearer {}");
        request.AddHeader("Content-Type", "application/json");

        var payload = new
        {
            model = "llama3-8b-8192",
            messages = new[] {
            new { role = "system", content = "You are the person whose resume is provided. Answer each question in 2-4 words only. Do not use full sentences. Be direct and brief." },
            new { role = "user", content = $"Resume:\n{_cachedResumeText}\n\nQuestion: {question}" }
        },
            temperature = 0.7
        };

        request.AddStringBody(JsonConvert.SerializeObject(payload), DataFormat.Json);

        var response = await client.ExecuteAsync(request);

        // Retry once if rate limited
        if ((int)response.StatusCode == 429)
        {
            Console.WriteLine("⏳ Rate limited. Retrying in 7 seconds...");
            await Task.Delay(7000);
            return await AskGroqAsync(question);
        }

        if (!response.IsSuccessful)
            return $"❌ Error: {response.StatusCode} - {response.Content}";

        dynamic json = JsonConvert.DeserializeObject(response.Content);
        return json?.choices?[0]?.message?.content?.ToString()?.Trim() ?? "❌ No answer found.";
    }

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
    private string ResolveAnswer(string question)
    {
        string q = question.ToLower();

        var user = new UserRepository().GetById(_userId); // fetch current user from DB

        if (q.Contains("current ctc")|| q.Contains("current salary")) return user.CurrentCtc ?? "";
        if (q.Contains("expected ctc") || q.Contains("expected salary")) return user.ExpectedCtc ?? "";
        if (q.Contains("notice period")) return user.NoticePeriod ?? "";
        if (q.Contains("how soon") || q.Contains("when can you join")) return user.JoinAvailability ?? "";

        return ""; // fallback if no match
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
