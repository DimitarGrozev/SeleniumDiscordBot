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

        public static async Task<int> GetBotCountAsync()
        {
            var bots = await GetBotListAsync();
            var count = bots.Count;

            return count;
        }

        public static async Task AddDiscordDataToBotAsync(DiscordData discordData, Guid id)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == id);

            if (bot != null)
            {
                bot.DiscordData = discordData;

                var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(targetsFilePath);

                if (file != null)
                {
                    var botsData = new BotsData { Bots = bots };
                    var serializedString = await Json.StringifyAsync(botsData);

                    await FileIO.WriteTextAsync(file, serializedString);
                }
            }
        }

        public static async Task<Messages> GetMessagesForBotAsync(Guid id)
        {
            var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(id + ".json");

            if (file != null)
            {
                var serializedString = await FileIO.ReadTextAsync(file);
                var messages = await Json.ToObjectAsync<Messages>(serializedString);

                if (messages != null)
                {
                    return messages;
                }
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
            var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(credentialsFilePath);

            if (file != null)
            {
                await FileIO.WriteTextAsync(file, serializedString);
            }
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
            var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(targetsFilePath);

            if (file != null)
            {
                var serializedString = await FileIO.ReadTextAsync(file);
                var botsData = await Json.ToObjectAsync<BotsData>(serializedString);

                if (botsData.Bots?.Count > 0)
                {
                    return botsData.Bots;
                }
            }

            return new List<Bot>();
        }

        public static async Task CreateMessagesForBotAsync(StorageFile chosenFile)
        {
            var fileContent = await FileIO.ReadLinesAsync(chosenFile);
            var messages = new Messages { Sentences = fileContent.ToList() };
            var serializedMessages = await Json.StringifyAsync(messages);

            var localMessagesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("temp" + ".json");
            await FileIO.WriteTextAsync(localMessagesFile, serializedMessages);
        }

        public static async Task DeleteMessagesForBotAsync(string fileName)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName + ".json");

            if (file != null)
            {
                await file.DeleteAsync();
            }
        }

        public static async Task DeleteMessagesForBotInCreationAsync()
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("temp.json");

            if (file != null)
            {
                await file.DeleteAsync();
            }
        }


        public static async Task<Bot> CreateBotAsync(string botName, string serverName, string channelName, int messageDelay, string email, string password, string token, string messageFileName, DiscordData discordData)
        {
            await ValidateBotNameUniqueness(botName);

            var bot = new Bot();
            var credentials = new Account { Email = email, Password = password, Token = token };

            bot.Id = Guid.NewGuid();
            bot.Name = botName;
            bot.Server = new Server { Name = serverName, Channel = channelName };
            bot.MessageDelay = messageDelay;
            bot.Credentials = credentials;
            bot.MessagesFileName = messageFileName;
            bot.DiscordData = discordData;

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

            await SaveMessagesForBot(bot.Id.ToString());

            return bot;
        }

        private static async Task ValidateBotNameUniqueness(string botName)
        {
            var bots = await GetBotListAsync();

            if (bots.Any(b => b.Name == botName))
            {
                throw new ArgumentException("Bot with such a name alreay exists!");
            }
        }

        private static async Task SaveMessagesForBot(string id)
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("temp.json");

            await file.RenameAsync(id + ".json");
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

                var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(targetsFilePath);

                if (file != null)
                {
                    var botsData = new BotsData { Bots = bots };
                    var serializedString = await Json.StringifyAsync(botsData);

                    await FileIO.WriteTextAsync(file, serializedString);
                    await DeleteMessagesForBotAsync(id.ToString());
                }
            }
        }

        public static async Task UpdateMessagesForBotAsync(string id, StorageFile file)
        {
            var fileContent = await FileIO.ReadLinesAsync(file);
            var messages = new Messages { Sentences = fileContent.ToList() };
            var serializedMessages = await Json.StringifyAsync(messages);

            var localMessagesFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(id + ".json", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(localMessagesFile, serializedMessages);
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
            var credentials = await ApplicationData.Current.LocalFolder.TryGetItemAsync(credentialsFilePath);

            return credentials != null;
        }

        public static async Task<Bot> GetBotByIdAsync(Guid id)
        {
            if (id != null)
            {
                var bots = await GetBotListAsync();
                var bot = bots.FirstOrDefault(b => b.Id == id);

                if (bot != null)
                {
                    return bot;
                }
            }

            throw new ArgumentException("No bot with such id was found!");
        }

        public static async Task EditBotAsync(string id, string botName, int messageDelay, string messagesFileName)
        {
            var bots = await GetBotListAsync();
            var bot = bots.FirstOrDefault(b => b.Id == Guid.Parse(id));

            if (bot != null)
            {
                bot.Name = botName;
                bot.MessageDelay = messageDelay;
                bot.MessagesFileName = messagesFileName;

                var file = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(targetsFilePath);

                if (file != null)
                {
                    var botsData = new BotsData { Bots = bots };
                    var serializedString = await Json.StringifyAsync(botsData);

                    await FileIO.WriteTextAsync(file, serializedString);

                    var shouldReplaceMessages = await CheckIfNewMessagesHaveBeenSelected();

                    if (shouldReplaceMessages)
                    {
                        await DeleteMessagesForBotAsync(id.ToString());
                        await SaveMessagesForBot(bot.Id.ToString());
                    }
                }
            }
        }

        private static async Task<bool> CheckIfNewMessagesHaveBeenSelected()
        {
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync("temp.json");

            return file != null;
        }
    }
}
