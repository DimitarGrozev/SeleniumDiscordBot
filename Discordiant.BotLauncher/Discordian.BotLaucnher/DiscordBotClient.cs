using Discordian.BotLauncher.Models;
using Discordian.BotLauncher.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Discordian.BotLauncher
{
	public class DiscordBotClient
	{
		private static readonly object loginLock = new object();

		private const string channelsList = "content-2a4AW9";
		private const string discordLoginUrl = "https://discord.com/login";
		private const string targetUrl = "https://discord.com/channels/@me";

		private readonly Messages messages;
		private readonly Bot botData;
		private readonly string email;
		private readonly string password;

		public DiscordBotClient(string email, string password)
		{
			this.email = email;
			this.password = password;
		}

		public DiscordBotClient(Messages messages, Bot botData)
		{
			this.messages = messages;
			this.botData = botData;
		}

		public void Launch(Object obj)
		{
			CancellationToken ct = (CancellationToken)obj;

			var chromeDriverService = ChromeDriverService.CreateDefaultService();
			chromeDriverService.HideCommandPromptWindow = true;
			var driver = new ChromeDriver(chromeDriverService);

			driver.Navigate().GoToUrl(targetUrl);
			Thread.Sleep(2000);

			this.Login(driver, botData.Credentials.Email, botData.Credentials.Password);

			this.NavigateToChannel(driver);

			while(!ct.IsCancellationRequested)
            {
				var message = messages.Sentences.OrderBy(m => Guid.NewGuid()).FirstOrDefault();
				var sanitizedMessage = Regex.Replace(message, @"\p{Cs}", " :D ");
				(new Actions(driver)).SendKeys(sanitizedMessage + " " + OpenQA.Selenium.Keys.Enter).Perform();

				Logger.LogMessage(botData.Server.Name, sanitizedMessage);

				//Delay between messages
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				while (true)
				{
					if (stopwatch.Elapsed.Milliseconds >= botData.MessageDelay || ct.IsCancellationRequested)
					{
						stopwatch.Stop();
						stopwatch.Reset();
						break;
					}
				}
			}

			driver.Quit();
			driver.Dispose();
			driver = null;
		}

		public string GetToken()
		{
			var token = string.Empty;

			var chromeDriverService = ChromeDriverService.CreateDefaultService();
			chromeDriverService.HideCommandPromptWindow = true;
			var options = new ChromeOptions();
			options.EnableMobileEmulation("iPhone 8");
			var driver = new ChromeDriver(chromeDriverService, options);
			driver.Navigate().GoToUrl(targetUrl);
			Thread.Sleep(2000);

			this.Login(driver, this.email, this.password);

			if (!driver.Url.Contains(discordLoginUrl))
			{
				token = (driver as IJavaScriptExecutor).ExecuteScript(Scripts.getDiscordTokenFromBrowser).ToString();
			}

			driver.Quit();
			driver.Dispose();
			driver = null;

			return token;
		}

		private void Login(IWebDriver driver, string email, string password)
		{
			lock (loginLock)
			{
				//Login
				var emailElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5 inputField-2RZxdl"));
				emailElement.SendKeys(email);

				var passwordElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5"));
				passwordElement.SendKeys(password);

				var loginButton = driver.FindElement(SelectorByAttributeValue("class", "marginBottom8-emkd0_ button-1cRKG6 button-f2h6uQ lookFilled-yCfaCM colorBrand-I6CyqQ sizeLarge-3mScP9 fullWidth-fJIsjq grow-2sR_-F"));
				loginButton.Click();

				Thread.Sleep(5000);

				while (true)
				{
					try
					{
						var verificationHeader = driver.FindElement(By.XPath("//iframe[contains(@src, 'captcha')]"));
						continue;
					}
					catch (Exception ex)
					{
						break;
					}
				}

				Thread.Sleep(2000);
			}
		}

		private void NavigateToChannel(IWebDriver driver)
		{
			//Find server
			var server = driver.FindElement(SelectorByAttributeThatContainsValue("aria-label", botData.Server.Name));
			server.Click();
			Thread.Sleep(1000);

			//Find channel
			var channelsSection = driver.FindElement(SelectorByAttributeValue("class", channelsList));
			channelsSection.Click();

			IWebElement channel = null;

			for (int i = 0; i < 100; i++)
			{
				new Actions(driver).SendKeys(Keys.Down).Build().Perform();

				try
				{
					channel = driver.FindElement(SelectorByAttributeThatContainsValue("aria-label", botData.Server.Channel));
					channel.Click();
				}
				catch (Exception)
				{
					continue;
				}

				break;
			}
		}

		private static By SelectorByAttributeValue(string p_strAttributeName, string p_strAttributeValue)
		{
			return (By.XPath(String.Format("//*[@{0} = '{1}']",
										   p_strAttributeName,
										   p_strAttributeValue)));
		}

		private static By SelectorByAttributeThatContainsValue(string p_strAttributeName, string p_strAttributeValue)
		{
			return (By.XPath(String.Format("//*[contains(@{0}, '{1}')]",
										   p_strAttributeName,
										   p_strAttributeValue)));
		}
	}
}
