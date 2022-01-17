using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Runtime;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Discordian.Services
{
    public static class DiscordianDbContext
    {
        private readonly static string targetsFilePath = "targets.json";
        private readonly static string appSettingsFilePath = "appSettings.json";
        private readonly static string credentialsFilePath = "credentials.json";
        private readonly static string messagesFilePath = "messages.json";

        public static async Task Initialize()
        {
            if (!await AppFilesExist())
            {
                await InitializeAppSettings();
                await InitializeMessages();
            }
        }

        public static async Task<Messages> GetAllMessagesAsync()
        {
            var appSettings = await GetAppSettingsAsync();
            var messagesPath = appSettings.MessagesFilePath;
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(messagesPath);
            var serializedString = await FileIO.ReadTextAsync(file);
            var messages = await Json.ToObjectAsync<Messages>(serializedString);

            if (messages != null)
            {
                return messages;
            }

            throw new ArgumentNullException("Messages could not be found!");
        }
        public static async Task<Bot> FindBotByIdAsync(Guid id)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == id);

            if (bot != null)
            {
                return bot;
            }

            throw new ArgumentNullException("No bot found with such Id!");
        }

        public static async Task<Credentials> GetCredentialsAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(credentialsFilePath, CreationCollisionOption.OpenIfExists);
            var serializedString = await FileIO.ReadTextAsync(file);
            var credentials = await Json.ToObjectAsync<Credentials>(serializedString);

            if (credentials != null)
            {
                return credentials;
            }

            throw new ArgumentNullException("Credentials could not be found!");
        }

        public static async Task<AppSettings> GetAppSettingsAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(appSettingsFilePath, CreationCollisionOption.OpenIfExists);
            var serializedString = await FileIO.ReadTextAsync(file);
            var appSettings = await Json.ToObjectAsync<AppSettings>(serializedString);

            if (appSettings != null)
            {
                return appSettings;
            }

            throw new ArgumentNullException("App settings could not be found!");
        }

        public static async Task<List<Bot>> GetBotListAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(targetsFilePath);
                var serializedString = await FileIO.ReadTextAsync(file);
                var botsData = await Json.ToObjectAsync<BotsData>(serializedString);

                if (botsData.Bots?.Count > 0)
                    return botsData.Bots;
            }
            catch (Exception e)
            {
                //TODO: add logging
            }

            return new List<Bot>();
        }

        public static async Task<Bot> CreateBotAsync(string botName, string serverName, string channelName, int messageDelay)
        {
            var bot = new Bot { Id = Guid.NewGuid(), Name = botName, Server = new Server { Name = serverName, Channel = channelName }, MessageDelay = messageDelay };

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(targetsFilePath, CreationCollisionOption.OpenIfExists);
            var serializedString = await FileIO.ReadTextAsync(file);
            var botsData = await Json.ToObjectAsync<BotsData>(serializedString);

            if (botsData == null)
            {
                botsData = new BotsData { Bots = new List<Bot>() };
            }

            botsData.Bots.Add(bot);

            var serializedBotsDataString = await Json.StringifyAsync(botsData);
            await FileIO.WriteTextAsync(file, serializedBotsDataString);

            return bot;
        }

        public static async Task DeleteBotById(Guid id)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == id);

            if(bot != null)
            {
                bots.Remove(bot);

                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(targetsFilePath);
                var botsData = new BotsData { Bots = bots };
                var serializedString = await Json.StringifyAsync(botsData);

                await FileIO.WriteTextAsync(file,serializedString);
            }
        }


        private static async Task InitializeAppSettings()
        {
            var appSettingsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(appSettingsFilePath, CreationCollisionOption.FailIfExists);
            var messagesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(messagesFilePath, CreationCollisionOption.FailIfExists);
            var appSettings = new AppSettings { MessagesFilePath = messagesFile.Path };

            await FileIO.WriteTextAsync(appSettingsFile, await Json.StringifyAsync(appSettings));
        }

        private static async Task InitializeMessages()
        {
            var messagesFile = await ApplicationData.Current.LocalFolder.GetFileAsync(messagesFilePath);
            var defaultMessagesString = File.ReadAllText("Data\\messages.json");

            await FileIO.WriteTextAsync(messagesFile, defaultMessagesString);
        }

        private static async Task<bool> AppFilesExist()
        {
            try
            {
                var appSettings = await ApplicationData.Current.LocalFolder.GetFileAsync(appSettingsFilePath);
                var messages = await ApplicationData.Current.LocalFolder.GetFileAsync(messagesFilePath);
            }
            catch(Exception)
            {
                return false;
            }

            return true;
        }

    }
}
