using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models;

namespace Discordian.Core.Services
{
    public static class DiscordianDbContext
    {
        private readonly static string targetFilePath = "Data/targets.json";
        private readonly static string appSettingsFilePath = "Data/configuration.json";
        private readonly static string credentialsFilePath = "Data/credentials.json";

        public static async Task<Credentials> GetCredentials()
        {
            var serializedString = File.ReadAllText(appSettingsFilePath);
            var credentials = await Json.ToObjectAsync<Credentials>(credentialsFilePath);

            return credentials;
        }

        public static async Task<AppSettings> GetAppSettings()
        {
            var serializedString = File.ReadAllText(appSettingsFilePath);
            var appSettings = await Json.ToObjectAsync<AppSettings>(serializedString);

            return appSettings;
        }

        public static async Task<IEnumerable<Bot>> GetBotList()
        {
            var serializedString = File.ReadAllText(targetFilePath);
            var botsData = await Json.ToObjectAsync<BotsData>(serializedString);

            if (botsData.Bots?.Count > 0)
                return botsData.Bots;

            return new List<Bot>();
        }

        public static async Task<Bot> CreateBotAsync(string botName, string serverName, string channelName, int messageDelay)
        {
            var bot = new Bot { Name = botName, Server = new Server { Name = serverName, Channel = channelName }, MessageDelay = messageDelay };

            var serializedString = File.ReadAllText(targetFilePath);
            var botsData = await Json.ToObjectAsync<BotsData>(serializedString);

            botsData.Bots.Add(bot);

            var serializedBotsDataString = await Json.StringifyAsync(botsData);

            File.WriteAllText("C:\\targets.json", serializedBotsDataString);

            return bot;
        }
    }
}
