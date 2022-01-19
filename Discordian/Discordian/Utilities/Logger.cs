using System;
using System.IO;

namespace Discordian.Utilities
{
    public static class Logger
    {
        private static readonly string path = "../../../Data";

        public static void LogMessage(string serverName, string message)
        {
            var formattedMessage = string.Format("[{0}]: {1}", DateTime.Now, message);

            using (StreamWriter sw = File.AppendText(path + $"/{serverName}-log.txt"))
            {
                sw.WriteLine(formattedMessage);
            }
        }
    }
}
