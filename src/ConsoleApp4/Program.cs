using ConsoleApp4.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using System;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConsoleApp4
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parameters
            var email = "discordtest123alabala@abv.bg";
            var pass = "asdQWE123!qjmihuq";
            var serverId = "guildsnav___923670412943044631";
            var channelsList = "content-2a4AW9";
            var channelId = "/channels/923670412943044631/923670551120207882";
            var questions = File.ReadAllLines("../../../messages.txt", System.Text.Encoding.UTF8).OrderBy(x => Guid.NewGuid());

            //Simulate activity
            var edgeBrowser = new EdgeDriver();
            edgeBrowser.Navigate().GoToUrl("https://discord.com/channels/@me");
            Thread.Sleep(2000);

            var emailElement = edgeBrowser.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5 inputField-2RZxdl"));
            emailElement.SendKeys(email);

            var passwordElement = edgeBrowser.FindElement(SelectorByAttributeValue("class", "inputDefault-3FGxgL input-2g-os5"));
            passwordElement.SendKeys(pass);

            var loginButton = edgeBrowser.FindElement(SelectorByAttributeValue("class", "marginBottom8-emkd0_ button-1cRKG6 button-f2h6uQ lookFilled-yCfaCM colorBrand-I6CyqQ sizeLarge-3mScP9 fullWidth-fJIsjq grow-2sR_-F"));
            loginButton.Click();
            Thread.Sleep(5000);

            var server = edgeBrowser.FindElement(SelectorByAttributeValue("data-list-item-id", serverId));
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
                    channel = edgeBrowser.FindElement(SelectorByAttributeValue("href", channelId));
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
                foreach (var question in questions)
                {
                    var message = Regex.Replace(question, @"\p{Cs}", " :D ");
                    (new Actions(edgeBrowser)).SendKeys(message + " " + OpenQA.Selenium.Keys.Enter).Perform();
                    Thread.Sleep(61000);
                }
            }
        }

        public static By SelectorByAttributeValue(string p_strAttributeName, string p_strAttributeValue)
        {
            return (By.XPath(String.Format("//*[@{0} = '{1}']",
                                           p_strAttributeName,
                                           p_strAttributeValue)));
        }
    }
}
