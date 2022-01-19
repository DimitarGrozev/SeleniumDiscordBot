using Discordian.BotLauncher.Models;
using Discordian.BotLauncher.Utilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace Discordian.BotLauncher
{
    public class DiscordBotClient
    {
        private const string channelsList = "content-2a4AW9";
        private const string targetUrl = "https://discord.com/channels/@me";

        private readonly Credentials credentials;
        private readonly AppSettings configuration;
        private readonly Messages messages;
        private readonly Bot botData;

        public DiscordBotClient(Credentials credentials, AppSettings configuration, Messages messages, Bot botData)
        {
            this.credentials = credentials;
            this.configuration = configuration;
            this.messages = messages;
            this.botData = botData;
        }

        public void Launch()
        {
            var driver = new EdgeDriver();
           
            driver.Navigate().GoToUrl(targetUrl);
            Thread.Sleep(2000);

            //Login
            var emailElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5 inputField-2RZxdl"));
            emailElement.SendKeys(this.credentials.Email);

            var passwordElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5"));
            passwordElement.SendKeys(this.credentials.Password);

            var loginButton = driver.FindElement(SelectorByAttributeValue("class", "marginBottom8-emkd0_ button-1cRKG6 button-f2h6uQ lookFilled-yCfaCM colorBrand-I6CyqQ sizeLarge-3mScP9 fullWidth-fJIsjq grow-2sR_-F"));
            loginButton.Click();
            Thread.Sleep(5000);

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

            foreach (var message in messages.Sentences)
            {
                var sanitizedMessage = Regex.Replace(message, @"\p{Cs}", " :D ");
                (new Actions(driver)).SendKeys(sanitizedMessage + " " + OpenQA.Selenium.Keys.Enter).Perform();

                Logger.LogMessage(botData.Server.Name, sanitizedMessage);

                //Delay between messages
                Thread.Sleep(botData.MessageDelay);
            }
        }

        public string Login()
        {
            var options = new ChromeOptions();
            options.EnableMobileEmulation("iPhone 8");
            var driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl(targetUrl);
            Thread.Sleep(2000);

            //Login
            var emailElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5 inputField-2RZxdl"));
            emailElement.SendKeys(this.credentials.Email);

            var passwordElement = driver.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5"));
            passwordElement.SendKeys(this.credentials.Password);

            var loginButton = driver.FindElement(SelectorByAttributeValue("class", "marginBottom8-emkd0_ button-1cRKG6 button-f2h6uQ lookFilled-yCfaCM colorBrand-I6CyqQ sizeLarge-3mScP9 fullWidth-fJIsjq grow-2sR_-F"));
            loginButton.Click();
            Thread.Sleep(5000);

            var token = (driver as IJavaScriptExecutor).ExecuteScript(Scripts.getDiscordTokenFromBrowser).ToString();

            driver.Close();

            return token;
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
