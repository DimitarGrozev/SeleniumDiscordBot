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
using Windows.Foundation.Collections;

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
                await InitializeCredentials();
            }
        }

        public static async Task<List<string>> GetSavedEmailsAsync()
        {
            var credentials = await GetCredentialsAsync();
            var emails = credentials.Accounts.Select(a => a.Email).ToList();

            return emails;
        }

        public static async Task LogoutAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(credentialsFilePath);
            await file.DeleteAsync();
        }

        public static async Task<int> GetBotCountAsync()
        {
            var bots = await GetBotListAsync();
            var count = bots.Count;

            return count;
        }

        public static async Task<Messages> GetAllMessagesAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(messagesFilePath);
            var serializedString = await FileIO.ReadTextAsync(file);
            var messages = await Json.ToObjectAsync<Messages>(serializedString);

            if (messages != null)
            {
                return messages;
            }

            throw new ArgumentNullException("Messages could not be found!");
        }

        public static async Task AddDiscordDataToBotAsync(DiscordData discordData, Guid id)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == id);

            if (bot != null)
            {
                bot.DiscordData = discordData;

                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(targetsFilePath);
                var botsData = new BotsData { Bots = bots };
                var serializedString = await Json.StringifyAsync(botsData);

                await FileIO.WriteTextAsync(file, serializedString);
            }
        }

        public static async Task<Messages> GetMessagesForBotAsync(Guid id)
        {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(id + ".json");
            var serializedString = await FileIO.ReadTextAsync(file);
            var messages = await Json.ToObjectAsync<Messages>(serializedString);

            if (messages != null)
            {
                return messages;
            }

            throw new ArgumentNullException("Messages could not be found!");
        }

        public static async Task<bool> AccountIsSavedAsync(string email, string password)
        {
            var credentials = await GetCredentialsAsync();
            var account = credentials.Accounts.FirstOrDefault(a => a.Email == email && a.Password == password);

            return account != null;
        }

        public static async Task<string> GetTokenForExistingAccountAsync(string email, string password)
        {
            var credentials = await GetCredentialsAsync();
            var account = credentials.Accounts.FirstOrDefault(a => a.Email == email && a.Password == password);

            if (account != null)
            {
                return account.Token;
            }

            return string.Empty;
        }

        public static async Task SaveAccountAsync(string email, string password, string token)
        {
            var credentials = await GetCredentialsAsync();
            var account = new Account { Email = email, Password = password, Token = token };

            credentials.Accounts.Add(account);

            var serializedString = await Json.StringifyAsync(credentials);
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(credentialsFilePath);
            await FileIO.WriteTextAsync(file, serializedString);
        }

        public static async Task<Account> AuthenticateAsync(string email, string password, string token)
        {
            var sanitizedToken = token.Replace("\"", "");
            var credentials = new Account { Email = email, Password = password, Token = sanitizedToken };
            var serializedString = await Json.StringifyAsync(credentials);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(credentialsFilePath, CreationCollisionOption.OpenIfExists);

            await FileIO.WriteTextAsync(file, serializedString);

            return credentials;
        }

        public static async Task<bool> IsAuthenticatedAsync()
        {
            var credentials = await GetCredentialsAsync();

            return credentials != null;
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

            return new Credentials();
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

        public static async Task<Guid> SaveMessagesForBotAsync(StorageFile chosenFile)
        {
            var fileContent = await FileIO.ReadLinesAsync(chosenFile);
            var messages = new Messages { Sentences = fileContent.ToList() };
            var serializedMessages = await Json.StringifyAsync(messages);
            var botId = Guid.NewGuid();

            var localMessagesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(botId + ".json");
            await FileIO.WriteTextAsync(localMessagesFile, serializedMessages);

            return botId;
        }

        public static async Task DeleteMessagesForBotAsync(string fileName)
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName + ".json");

                await file.DeleteAsync();
            }
            catch (Exception) { }
        }

        public static async Task<Bot> CreateBotAsync(string id, string botName, string serverName, string channelName, int messageDelay, string email, string password, string token)
        {
            var bot = new Bot();
            var credentials = new Account { Email = email, Password = password, Token = token };

            bot.Id = Guid.Parse(id);
            bot.Name = botName;
            bot.Server = new Server { Name = serverName, Channel = channelName };
            bot.MessageDelay = messageDelay;
            bot.Credentials = credentials;

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

        public static async Task<string> GetPasswordForAccountAsync(string email)
        {
            var credentials = await GetCredentialsAsync();
            var account = credentials.Accounts.FirstOrDefault(a => a.Email == email);

            if (account != null)
            {
                return account.Password;
            }

            return string.Empty;
        }

        public static async Task DeleteBotById(Guid id)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == id);

            if (bot != null)
            {
                bots.Remove(bot);

                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(targetsFilePath);
                var botsData = new BotsData { Bots = bots };
                var serializedString = await Json.StringifyAsync(botsData);

                await FileIO.WriteTextAsync(file, serializedString);
                await DeleteMessagesForBotAsync(id.ToString());
            }
        }

        private static async Task InitializeCredentials()
        {
            var appSettingsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(credentialsFilePath, CreationCollisionOption.FailIfExists);
            var crendentials = new Credentials { Accounts = new List<Account>() };
            var serializedString = await Json.StringifyAsync(crendentials);

            await FileIO.WriteTextAsync(appSettingsFile, serializedString);
        }

        private static async Task<bool> AppFilesExist()
        {
            try
            {
                var credentials = await ApplicationData.Current.LocalFolder.GetFileAsync(credentialsFilePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

    }
}
