using ConsoleApp4.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
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
        private const string channelsList = "content-2a4AW9";
        private const string targetUrl = "https://discord.com/channels/@me";

        private readonly Credentials credentials;
        private readonly Configuration configuration;
        private readonly Messages messages;
        private readonly Targets targets;

        public DiscordBotClient(Credentials credentials, Configuration configuration, Messages messages, Targets targets)
        {
            this.credentials = credentials;
            this.configuration = configuration;
            this.messages = messages;
            this.targets = targets;
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

            //Start messaging the servers
            while (true)
            {
                foreach (var target in targets.Servers)
                {
                    var server = driver.FindElement(SelectorByAttributeThatContainsValue("aria-label", target.Name));
                    server.Click();
                    Thread.Sleep(1000);

                    var channelsSection = driver.FindElement(SelectorByAttributeValue("class", channelsList));
                    channelsSection.Click();

                    IWebElement channel = null;

                    for (int i = 0; i < 100; i++)
                    {
                        new Actions(driver).SendKeys(Keys.Down).Build().Perform();

                        try
                        {
                            channel = driver.FindElement(SelectorByAttributeThatContainsValue("aria-label", target.Channel));
                            channel.Click();
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        break;
                    }

                    var message = this.messages.Sentences.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                    var sanitizedMessage = Regex.Replace(message, @"\p{Cs}", " :D ");
                    (new Actions(driver)).SendKeys(sanitizedMessage + " " + OpenQA.Selenium.Keys.Enter).Perform();

                    //Wait between server switch
                    Thread.Sleep(this.configuration.ServerSwitchDelay);
                }

                //Delay between messages
                Thread.Sleep(this.configuration.MessageDelay);
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
