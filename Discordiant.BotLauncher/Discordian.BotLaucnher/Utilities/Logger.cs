using System;
using System.IO;
using Windows.Storage;

namespace Discordian.BotLauncher.Utilities
{
    public static class Logger
    {
        public static async void LogMessage(string serverName, string message)
        {
            var formattedMessage = string.Format("[{0}]: {1}", DateTime.Now, message);
            var fileName = $"{serverName}-log.txt";
            var documentsFolder =  KnownFolders.DocumentsLibrary;
            var discordianLogs = await documentsFolder.CreateFolderAsync("Discordian Logs", CreationCollisionOption.OpenIfExists);
            var logFile = await discordianLogs.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            await FileIO.AppendTextAsync(logFile, formattedMessage);
        }
    }
}
