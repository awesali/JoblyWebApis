using JoblyWebApi.Repositories;
using JoblyWebApi.Services;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using SeleniumExtras.WaitHelpers;
using UglyToad.PdfPig;
using joblywebapi.Models;
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
    private bool CheckSuccessMessage(IWebDriver driver)
    {
        try
        {
            return driver.PageSource.ToLower().Contains("you have successfully applied");
        }
        catch
        {
            return false;
        }
    }
    private List<string> resumeSkills = new();

    public async void Run()

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
            // 🧠 Extract resume text from PDF
            string resumePath = new ResumeRepository().GetResumePath(_userId);
            if (string.IsNullOrEmpty(resumePath) || !System.IO.File.Exists(resumePath))
            {
                Console.WriteLine("❌ Resume not found for skill match.");
                return;
            }

            string resumeText = ExtractTextFromPdf(resumePath);

            // 🔎 Ask Groq to extract skill list dynamically
            string skillQuestion = "List all technical skills mentioned in this resume. Return only comma-separated values like: skill1, skill2, skill3.\r\n";
            string skillAnswer = await AskGroqAsync($"Resume:\n{resumeText}\n\nQuestion: {skillQuestion}");

            resumeSkills = skillAnswer
                .ToLower()
                .Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            Console.WriteLine("🎯 Resume Skills: " + string.Join(", ", resumeSkills));


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
            bool successMessageFound = false;  // ✅ Add this line

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
            // ✅ Extract key skills from job card itself (before clicking)
            List<string> jobPageSkills = new();
            try
            {
                var skillTags = job.FindElements(By.CssSelector("ul.tags li"));
                jobPageSkills = skillTags.Select(s => s.Text.Trim().ToLower())
                                         .Where(s => !string.IsNullOrEmpty(s))
                                         .Distinct()
                                         .ToList();
                Console.WriteLine("🧩 Job Card Skills: " + string.Join(", ", jobPageSkills));
            }
            catch
            {
                Console.WriteLine("⚠ Could not extract skills from job card.");
            }

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
            
            // ✅ Match resume skills with job key skills
            int matchedSkills = resumeSkills.Count(rs =>
                jobPageSkills.Any(js => js.Contains(rs) || rs.Contains(js))
            );

            if (matchedSkills < 2)
            {
                Console.WriteLine($"❌ Only {matchedSkills} skill(s) matched. Skipping job.");
                driver.Close();
                driver.SwitchTo().Window(originalWindow);
                return;
            }

            Console.WriteLine($"✅ Skill match passed with {matchedSkills} skill(s)");

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
            // 🔍 Check success message immediately after clicking
            successMessageFound = CheckSuccessMessage(driver);
            if (successMessageFound)
                Console.WriteLine("🎉 Success message found immediately after clicking Apply");

            // Scenario 1: Check if success message appears
            //bool successMessageFound = false;
            //try
            //{
            //    successMessageFound = driver.PageSource.ToLower().Contains("applied successfully")
            //        || driver.FindElements(By.XPath("//*[contains(text(), 'successfully applied')]")).Any();

            //    if (successMessageFound)
            //        Console.WriteLine("🎉 Job applied successfully message found.");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("⚠ Error checking success message: " + ex.Message);
            //}

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

                                // ✅ Check again after Q&A
                                successMessageFound = CheckSuccessMessage(driver);
                                if (successMessageFound)
                                    Console.WriteLine("🎉 Success message found after completing QnA");
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
        request.AddHeader("Authorization", $"Bearer {_apiKey}");
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