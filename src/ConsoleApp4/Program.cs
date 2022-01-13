using ConsoleApp4.Models;
using ConsoleApp4.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        private static readonly string configurationFilePath = "../../../Data/configuration.json";
        private static readonly string credentialsFilePath = "../../../Data/credentials.json";
        private static readonly string targetsFilePath = "../../../Data/targets.json";

        static void Main(string[] args)
        {
            var configurationProvider = new ServiceProvider<Configuration>(configurationFilePath);
            var configuration = configurationProvider.Provide();

            var credentialsProvider = new ServiceProvider<Credentials>(credentialsFilePath);
            var credentials = credentialsProvider.Provide();

            var targetsProvider = new ServiceProvider<Targets>(targetsFilePath);
            var targets = targetsProvider.Provide();

            var messagesProvider = new ServiceProvider<Messages>(configuration.MessagesFilePath);
            var messages = messagesProvider.Provide();
           
            var client = new DiscordBotClient(credentials,configuration,messages,targets);
            client.Launch();
        }
    }
}
