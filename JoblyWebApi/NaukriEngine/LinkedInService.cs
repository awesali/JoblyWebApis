using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Data;
using System.Data.SqlClient;

namespace joblywebapi.Services
{
    public class LinkedInService
    {
        private readonly IConfiguration _config;

        public LinkedInService(IConfiguration config)
        {
            _config = config;
        }

        public string RunLinkedInAutomation()
        {
            string jobSearchUrl = _config["LinkedIn:JobSearchUrl"]!;
            string profileBasePath = _config["LinkedIn:ChromeProfileBasePath"]!;
            string email = _config["LinkedIn:Email"]!;
            string password = _config["LinkedIn:Password"]!;
            string resumePath = _config["LinkedIn:ResumePath"]!;
            string connString = _config.GetConnectionString("InterviewDb")!;

            IWebDriver driver = null!;
            try
            {
                driver = CreateOrUsePersistentChromeProfile(profileBasePath);

                // Open LinkedIn
                driver.Navigate().GoToUrl("https://www.linkedin.com");

                // Login check
                if (IsLoggedIn(driver))
                {
                    Console.WriteLine("✅ Already logged in.");
                }
                else
                {
                    Console.WriteLine("🔐 Logging in...");
                    driver.Navigate().GoToUrl("https://www.linkedin.com/login");
                    Thread.Sleep(2000);

                    driver.FindElement(By.Id("username")).SendKeys(email);
                    driver.FindElement(By.Id("password")).SendKeys(password);
                    Thread.Sleep(2000);
                    driver.FindElement(By.XPath("//button[@type='submit']")).Click();
                    Thread.Sleep(3000);
                }

            NewSearch:
                driver.Navigate().GoToUrl(jobSearchUrl);
                Thread.Sleep(5000);

                WebDriverWait wait1 = new(driver, TimeSpan.FromSeconds(10));
                IWebElement resultCountElement = wait1.Until(ExpectedConditions.ElementIsVisible(
                    By.XPath("//div[contains(@class, 'jobs-search-results-list__subtitle')]//span")));

                string resultText = resultCountElement.Text;
                Console.WriteLine($"Result Text: {resultText}");

                var jobCards = driver.FindElements(By.ClassName("job-card-container"));
                Console.WriteLine($"Found {jobCards.Count} jobs.");

                int appliedCount = 0;

                foreach (var job in jobCards)
                {
                    try
                    {
                        job.Click();
                        Thread.Sleep(5000);

                        var easyApplyButton = driver.FindElement(By.CssSelector("button.jobs-apply-button"));
                        string buttonText = easyApplyButton.Text.Trim();
                        if (buttonText.ToLower() != "easy apply") continue;

                        easyApplyButton.Click();
                        WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

                        var inputs = driver.FindElements(By.XPath("//input[not(@type='hidden')]"));
                        foreach (var input in inputs)
                        {
                            try
                            {
                                string labelText = GetLabelTextForInputField(driver, input);
                                if (labelText.ToLower() == "irst name")
                                {
                                    goto NewSearch;
                                }
                            }
                            catch
                            {
                                goto NewSearch;
                            }
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            try
                            {
                                var nextButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[normalize-space()='Next']")));
                                if (nextButton != null && nextButton.Displayed && nextButton.Enabled)
                                {
                                    nextButton.Click();
                                    Thread.Sleep(2000);
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }

                        FillFormFields(driver, resumePath, connString);

                        try
                        {
                            var submitButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Done']")));
                            if (submitButton != null)
                            {
                                submitButton.Click();
                                Console.WriteLine("✅ Form submitted.");
                            }
                        }
                        catch
                        {
                            goto NewSearch;
                        }

                        driver.Navigate().Back();
                        Thread.Sleep(3000);

                        appliedCount++;
                        if (appliedCount % 5 == 0)
                        {
                            Console.WriteLine("Taking a break for 1 minute...");
                            Thread.Sleep(60000);
                        }

                        goto NewSearch;
                    }
                    catch (NoSuchElementException)
                    {
                        Console.WriteLine("❌ No 'Easy Apply' button. Skipping.");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error: {ex.Message}");
                        driver.Navigate().Back();
                        Thread.Sleep(3000);
                    }
                }

                driver.Quit();
                return "✅ LinkedIn automation completed.";
            }
            catch (Exception ex)
            {
                try { driver?.Quit(); } catch { }
                return $"❌ FAILED: {ex.Message}";
            }
        }

        // ================= Helpers =================

        private static bool IsLoggedIn(IWebDriver driver)
        {
            return driver.Url.Contains("linkedin.com/feed") || driver.Url.Contains("linkedin.com/jobs");
        }

        private void FillFormFields(IWebDriver driver, string resumePath, string connectionString)
        {
            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));

            bool isFormCompleted = false;
            int step = 1;

            while (!isFormCompleted)
            {
                try
                {
                    var inputs = driver.FindElements(By.XPath("//input[not(@type='hidden')]"));
                    foreach (var input in inputs)
                    {
                        try
                        {
                        ReCheck:
                            string labelText = GetLabelTextForInputField(driver, input);
                            string inputValue = GetAnswerFromDatabase(connectionString, labelText);
                            Console.WriteLine($"Step {step}...{labelText}_{inputValue}");
                            if (inputValue == "NA")
                            {
                                goto ReCheck;
                            }
                            if (string.IsNullOrEmpty(input.GetAttribute("value")))
                            {
                                input.Clear();
                                input.SendKeys(inputValue);
                            }
                        }
                        catch { }
                        try
                        {
                            var locationInput = driver.FindElement(By.Id("single-typeahead-entity-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-4214402496-18632087244-location-GEO-LOCATION"));
                            locationInput.Clear();
                            locationInput.SendKeys("Nagpur, Maharashtra, India");
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    var fieldsets = driver.FindElements(By.XPath("//fieldset[@data-test-form-builder-radio-button-form-component='true']"));

                    foreach (var fieldset in fieldsets)
                    {
                        try
                        {
                            var legend = fieldset.FindElement(By.TagName("legend"));
                            string questionText = legend.Text.Trim();
                            string answer = GetAnswerFromDatabase(connectionString, questionText);

                            if (string.IsNullOrEmpty(answer))
                                continue;

                            var options = fieldset.FindElements(By.XPath(".//input[@type='radio']"));

                            foreach (var option in options)
                            {
                                var label = fieldset.FindElement(By.XPath($".//label[@for='{option.GetAttribute("id")}']"));
                                string labelText = label.Text.Trim();

                                if (labelText.Equals(answer, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!option.Selected)
                                        option.Click();

                                    Console.WriteLine($"✅ Selected '{answer}' for question: {questionText}");
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error processing fieldset: {ex.Message}");
                        }
                    }
                }
                catch (Exception outerEx)
                {
                    Console.WriteLine($"🔥 Outer error: {outerEx.Message}");
                }

                try
                {
                    var checkboxes = driver.FindElements(By.XPath("//input[@type='checkbox']"));
                    foreach (var box in checkboxes)
                    {
                        try
                        {
                            if (!box.Selected)
                            {
                                box.Click();
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    var fromYearSelect = new SelectElement(driver.FindElement(By.Id("date-range-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-4200989327-8900184304861770401-dateRange-range-start-date-year-select")));
                    fromYearSelect.SelectByValue("2014");

                    var toYearSelect = new SelectElement(driver.FindElement(By.Id("date-range-form-component-formElement-urn-li-jobs-applyformcommon-easyApplyFormElement-4200989327-8900184304861770401-dateRange-range-end-date-year-select")));
                    toYearSelect.SelectByValue("2018");
                }
                catch { }

                try
                {
                    var labels = driver.FindElements(By.XPath("//label"));

                    foreach (var label in labels)
                    {
                    ReCheck:
                        string questionText = label.Text.Trim();
                        if (string.IsNullOrEmpty(questionText))
                            continue;

                        string answer = GetAnswerFromDatabase(connectionString, questionText);
                        if (answer == "NA")
                        {
                            goto ReCheck;
                        }
                        if (string.IsNullOrEmpty(answer))
                            continue;

                        var selectId = label.GetAttribute("for");
                        if (string.IsNullOrEmpty(selectId))
                            continue;

                        try
                        {
                            var selectElement = new SelectElement(driver.FindElement(By.Id(selectId)));
                            selectElement.SelectByText(answer);
                            Console.WriteLine($"✅ Selected '{answer}' for '{questionText}'");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Could not select '{answer}' for '{questionText}': {ex.Message}");
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    var selects = driver.FindElements(By.TagName("select"));
                    foreach (var select in selects)
                    {
                        try
                        {
                            var dropdown = new SelectElement(select);
                            if (dropdown.Options.Count > 1)
                            {
                                dropdown.SelectByIndex(1);
                            }
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    var uploadButton = driver.FindElement(By.CssSelector("input[type='file']"));
                    if (uploadButton.Displayed)
                    {
                        uploadButton.SendKeys(resumePath);
                        Thread.Sleep(2000);
                    }
                }
                catch { }

                try
                {
                    var nextButton = driver.FindElement(By.XPath("//button[normalize-space()='Next']"));
                    if (nextButton != null && nextButton.Displayed && nextButton.Enabled)
                    {
                        nextButton.Click();
                        Thread.Sleep(2000);
                        FillFormFields(driver, resumePath, connectionString);
                        step++;
                        continue;
                    }
                }
                catch { }

                try
                {
                    var reviewButton = driver.FindElement(By.XPath("//button[normalize-space()='Review']"));
                    if (reviewButton != null && reviewButton.Displayed && reviewButton.Enabled)
                    {
                        reviewButton.Click();
                        Thread.Sleep(2000);
                    }
                }
                catch { }

                isFormCompleted = true;
            }

            try
            {
                var submitButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Submit application']")));
                if (submitButton != null)
                {
                    submitButton.Click();
                    Console.WriteLine("✅ Form submitted.");
                }

                Thread.Sleep(3000);
                var DoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(
              By.XPath("//button[.//span[text()='Done']]")));

                if (DoneButton != null)
                {
                    DoneButton.Click();
                    Console.WriteLine("✅ Form Done.");
                }
            }
            catch
            {
            }
        }

        private static string GetLabelTextForInputField(IWebDriver driver, IWebElement input)
        {
            string label = input.GetAttribute("aria-label");
            if (!string.IsNullOrEmpty(label))
                return label.Trim();

            label = input.GetAttribute("placeholder");
            if (!string.IsNullOrEmpty(label))
                return label.Trim();

            try
            {
                var labelElement = input.FindElement(By.XPath("preceding-sibling::label"));
                if (labelElement != null)
                    return labelElement.Text.Trim();
            }
            catch { }

            try
            {
                var parentLabel = input.FindElement(By.XPath("ancestor::div[1]//label"));
                if (parentLabel != null)
                    return parentLabel.Text.Trim();
            }
            catch { }

            try
            {
                var sibling = input.FindElement(By.XPath("./preceding-sibling::*[1]"));
                if (sibling != null)
                    return sibling.Text.Trim();
            }
            catch { }

            try
            {
                var subtitleSpan = input.FindElement(By.XPath("ancestor::div[contains(@class, 'rEPiArwEebvLErdSBBqQJmLGsDgrBmSVArkmw')]/preceding-sibling::span[contains(@class, 'jobs-easy-apply-form-section__group-subtitle')]"));
                if (subtitleSpan != null)
                    return subtitleSpan.Text.Trim();
            }
            catch { }

            return string.Empty;
        }

        private static string GetAnswerFromDatabase(string connectionString, string question)
        {
            string answer = string.Empty;

            using SqlConnection conn = new(connectionString);
            using SqlCommand cmd = new("usp_GetOrInsertEsayApplyAnswer", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Question", question);

            SqlParameter outputParam = new("@Answer", SqlDbType.NVarChar, -1)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            conn.Open();
            cmd.ExecuteNonQuery();

            answer = outputParam.Value?.ToString();
            return answer;
        }

        private static IWebDriver CreateOrUsePersistentChromeProfile(string baseProfilePath, string profileName = "LinkedInBotProfile")
        {
            string fullProfilePath = Path.Combine(baseProfilePath, profileName);

            if (!Directory.Exists(fullProfilePath))
            {
                Directory.CreateDirectory(fullProfilePath);
            }

            ChromeOptions options = new();
            options.AddArgument($"--user-data-dir={fullProfilePath}");
            options.AddArgument("--remote-debugging-port=9222");
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");

            try
            {
                return new ChromeDriver(options);
            }
            catch
            {
                return CreateTemporaryChromeProfile();
            }
        }

        private static IWebDriver CreateTemporaryChromeProfile()
        {
            string tempProfileDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempProfileDir);

            TimeSpan timeout = TimeSpan.FromSeconds(120);

            ChromeOptions options = new();
            options.AddArgument($"--user-data-dir={tempProfileDir}");
            options.AddArgument("--start-maximized");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();

            return new ChromeDriver(service, options, timeout);
        }
    }
}
