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

            var channelId = "/channels/923670412943044631/923670551120207882";
            var questions = File.ReadAllLines("../../../messages.txt", System.Text.Encoding.UTF8).OrderBy(x => Guid.NewGuid());

            var client = new DiscordBotClient(email, pass, "Squiggles", "squiggles-chat", 61000, questions);

            client.Launch();
        }
    }
}
