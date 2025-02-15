﻿using System;
using System.Linq;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using System.Collections.Generic;
using Discordian.BotLauncher.Models;
using Discordian.BotLauncher.Utilities;
using System.Text.RegularExpressions;

namespace Discordian.BotLauncher
{
	class Program
	{
		static Dictionary<Guid, Thread> RunningThreads = new Dictionary<Guid, Thread>();
		static Dictionary<Guid, CancellationTokenSource> CancellationTokens = new Dictionary<Guid, CancellationTokenSource>();

		static AppServiceConnection connection = null;

		/// <summary>
		/// Creates an app service thread
		/// </summary>
		static void Main(string[] args)
		{
			var appServiceExit = new AutoResetEvent(false);

			Thread appServiceThread = new Thread(new ThreadStart(ThreadProc));
			appServiceThread.Start();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine("*****************************");
			Console.WriteLine("**** Discord bot launcher ****");
			Console.WriteLine("*****************************");
			Console.ReadLine();

			appServiceExit.WaitOne();
		}

		/// <summary>
		/// Creates the app service connection
		/// </summary>
		static async void ThreadProc()
		{
			connection = new AppServiceConnection();
			connection.AppServiceName = "UWPApp";
			connection.PackageFamilyName = "0E3B9D02-236F-4DFA-9A4A-6A32D4873B17_5nm7g4p6413wg";
			connection.RequestReceived += Connection_RequestReceived;
			connection.ServiceClosed += Connection_Closed;

			AppServiceConnectionStatus status = await connection.OpenAsync();

			switch (status)
			{
				case AppServiceConnectionStatus.Success:
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Connection established - waiting for requests");
					Console.WriteLine();
					break;
				case AppServiceConnectionStatus.AppNotInstalled:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("The app AppServicesProvider is not installed.");
					return;
				case AppServiceConnectionStatus.AppUnavailable:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("The app AppServicesProvider is not available.");
					return;
				case AppServiceConnectionStatus.AppServiceUnavailable:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", connection.AppServiceName));
					return;
				case AppServiceConnectionStatus.Unknown:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(string.Format("An unkown error occurred while we were trying to open an AppServiceConnection."));
					return;
			}
		}

		private static void Connection_Closed(AppServiceConnection sender, AppServiceClosedEventArgs args)
		{
			Environment.Exit(1);
		}

		/// <summary>
		/// Receives message from UWP app and sends a response back
		/// </summary>
		private static async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
		{
			var key = args.Request.Message.FirstOrDefault(m => m.Key == "request").Value.ToString();

			Console.ForegroundColor = ConsoleColor.Cyan;

			if (key == "login")
			{
				var email = args.Request.Message.FirstOrDefault(m => m.Key == "email").Value.ToString();
				var password = args.Request.Message.FirstOrDefault(m => m.Key == "password").Value.ToString();

				Console.WriteLine(string.Format("Received request to get token from browser"));

				var discordBot = new DiscordBotClient(email, password);
				var token = discordBot.GetToken();

				var message = new ValueSet();
				message.Add("token", token);

				await args.Request.SendResponseAsync(message);
			}
			else if(key == "start")
			{
				var bot = await Json.ToObjectAsync<Bot>(args.Request.Message["bot"].ToString());
				var messages = await Json.ToObjectAsync<Messages>(args.Request.Message["messages"].ToString());

				Console.WriteLine(string.Format("Received request to start bot {0}", bot.Name));

				var discordBot = new DiscordBotClient(messages, bot);
				var cts = new CancellationTokenSource();
				var thread = new Thread(new ParameterizedThreadStart(discordBot.Launch));

				thread.Start(cts.Token);

				RunningThreads.Add(bot.Id, thread);
				CancellationTokens.Add(bot.Id, cts);
				Console.WriteLine("Launched bot");

				var response = new ValueSet();
				response.Add("received", true);

				var botStartResponse = connection.SendMessageAsync(response);
			}
			else if(key == "stop")
            {
				var stringId = args.Request.Message.FirstOrDefault(m => m.Key == "id").Value.ToString();

				var sanitizedStringId = stringId.Replace("\"", "");

				var parsedId = Guid.Parse(sanitizedStringId);

				CancellationTokens[parsedId].Cancel();

				CancellationTokens.Remove(parsedId);
				RunningThreads.Remove(parsedId);

				Console.WriteLine("Stopped bot");

				var response = new ValueSet();
				response.Add("received", true);

				var botStopResponse = connection.SendMessageAsync(response);
			}
			else if (key == "stopAll")
			{
				var allKeys = CancellationTokens.Keys.ToList();

				foreach (var botId in CancellationTokens.Keys)
				{
					CancellationTokens[botId].Cancel();

					CancellationTokens.Remove(botId);
					RunningThreads.Remove(botId);
				}

				Console.WriteLine("Stopped all bots");

				var response = new ValueSet();
				response.Add("received", true);

				var botStopAllResponse = connection.SendMessageAsync(response);
			}
		}
	}
}