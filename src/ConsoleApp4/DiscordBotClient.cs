using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public class DiscordBotClient
    {
        private readonly string email;
        private readonly string password;
        private readonly string serverName;
        private readonly string channelName;
        private readonly int messageDelay;
        private readonly IEnumerable<string> messages;

        private const string channelsList = "content-2a4AW9";
        private const string targetUrl = "https://discord.com/channels/@me";

        public DiscordBotClient(string email, string password, string serverName, string channelName, int messageDelay, IEnumerable<string> messages)
        {
            this.email = email;
            this.password = password;
            this.serverName = serverName;
            this.channelName = channelName;
            this.messageDelay = messageDelay;
            this.messages = messages;
        }

        public void Launch()
        {
            //Simulate activity
            var edgeBrowser = new EdgeDriver();
            edgeBrowser.Navigate().GoToUrl("https://discord.com/channels/@me");
            Thread.Sleep(2000);

            var emailElement = edgeBrowser.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5 inputField-2RZxdl"));
            emailElement.SendKeys(this.email);

            var passwordElement = edgeBrowser.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5"));
            passwordElement.SendKeys(this.password);

            var loginButton = edgeBrowser.FindElement(SelectorByAttributeValue("class", "marginBottom8-emkd0_ button-1cRKG6 button-f2h6uQ lookFilled-yCfaCM colorBrand-I6CyqQ sizeLarge-3mScP9 fullWidth-fJIsjq grow-2sR_-F"));
            loginButton.Click();
            Thread.Sleep(5000);

            var server = edgeBrowser.FindElement(SelectorByAttributeThatContainsValue("aria-label", serverName));
            server.Click();
            Thread.Sleep(1000);

            var channelsSection = edgeBrowser.FindElement(SelectorByAttributeValue("class", channelsList));
            channelsSection.Click();

            IWebElement channel = null;

            for (int i = 0; i < 100; i++)
            {
                new Actions(edgeBrowser).SendKeys(Keys.Down).Build().Perform();

                try
                {
                    channel = edgeBrowser.FindElement(SelectorByAttributeThatContainsValue("aria-label", channelName));
                    channel.Click();
                }
                catch (Exception e)
                {
                    continue;
                }

                break;
            }

            while (true)
            {
                foreach (var message in messages)
                {
                    var sanitizedMessage = Regex.Replace(message, @"\p{Cs}", " :D ");
                    (new Actions(edgeBrowser)).SendKeys(sanitizedMessage + " " + OpenQA.Selenium.Keys.Enter).Perform();
                    Thread.Sleep(this.messageDelay);
                }
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
