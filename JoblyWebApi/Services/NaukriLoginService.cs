using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading.Tasks;

namespace JoblyWebApi.Services
{
    public class NaukriLoginService
    {
        public async Task<bool> TryLoginAsync(string username, string password)
        {
            var options = new ChromeOptions();
            // ❌ REMOVE HEADLESS
            // options.AddArguments("--headless", "--no-sandbox");
            using (var driver = new ChromeDriver(options))
            {
                try
                {
                    driver.Navigate().GoToUrl("https://www.naukri.com/mnjuser/login");
                    await Task.Delay(3000);

                    driver.FindElement(By.Id("usernameField")).SendKeys(username);
                    driver.FindElement(By.Id("passwordField")).SendKeys(password);
                    driver.FindElement(By.XPath("//button[@type='submit']")).Click();
                    // After login
                    await Task.Delay(4000);

                    bool isLoginSuccess = false;

                    try
                    {
                        var profileIcon = driver.FindElement(By.XPath("//div[@class='nI-gNb-drawer__icon']"));
                        if (profileIcon != null)
                        {
                            isLoginSuccess = true;
                        }
                    }
                    catch
                    {
                        isLoginSuccess = false;
                    }

                    return isLoginSuccess;

                }
                catch
                {
                    return false;
                }
                finally
                {
                    driver.Quit();
                }
            }
        }
    }
}
