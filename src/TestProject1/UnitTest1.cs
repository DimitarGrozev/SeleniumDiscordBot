using ConsoleApp4;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestProject1
{
    [TestFixture(typeof(ChromeDriver))]
    [TestFixture(typeof(EdgeDriver))]
    public class BlogTest<TWebDriver> where TWebDriver : IWebDriver, new()
    {
        private IWebDriver driver;
        private static readonly string email = "sodovag55@abv.bg";
        private static readonly string pass = "asdQWE123!qjmihuq";
        private static readonly string serverName = "Squiggles";
        private static readonly string channelName = "squiggles-chat";
        private static readonly string chrome = "chrome";
        private static readonly string edge = "edge";
        private static readonly int messageDelay = 61000;
        private static readonly IEnumerable<string> questions = File.ReadAllLines("../../../../ConsoleApp4/messages.txt", System.Text.Encoding.UTF8).OrderBy(x => Guid.NewGuid());

        [Test]
        public void Can_Visit_Google()
        {
            driver = new TWebDriver();

            var bot = new DiscordBotClient(email, pass, serverName, channelName, messageDelay, questions, driver);
            bot.Launch();
        }
    }
}