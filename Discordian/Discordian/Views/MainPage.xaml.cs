using System;
using System.Collections.Generic;
using System.Linq;
using Discordian.Core.Models.Charts;
using Discordian.Services;
using Discordian.Utilities;
using Discordian.ViewModels;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discordian.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Connection == null)
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");
            }

            ProgressSpinnerForMessagesPerBot.IsActive = true;
            ProgressSpinnerForMentionsPerBot.IsActive = true;

            var bots = await DiscordianDbContext.GetBotListAsync();
            var messageCountChartData = new List<Data>();
            var mentionsCountChartData = new List<Data>();
            var currentBot = string.Empty;

            try
            {
                foreach (var bot in bots)
                {
                    currentBot = bot.Name;

                    var token = bot.Credentials.Token;
                    var discordData = bot.DiscordData;

                    if (discordData == null)
                    {
                        discordData = await DiscordApiClient.GetDiscordDataForBot(bot.Server.Name, bot.Server.Channel, bot.Credentials.Token);
                        await DiscordianDbContext.AddDiscordDataToBotAsync(discordData, bot.Id);
                    }

                    var messageCount = await DiscordApiClient.GetBotMessageCountInChannelAsync(discordData, token);
                    var mentionsCount = await DiscordApiClient.GetBotMentionsCountInChannelAsync(discordData, token);

                    messageCountChartData.Add(new Data { Category = bot.Name, Value = messageCount, LabelProperty = messageCount.ToString() });
                    mentionsCountChartData.Add(new Data { Category = bot.Name, Value = mentionsCount, LabelProperty = mentionsCount.ToString() });
                }

                this.barSeries.DataContext = messageCountChartData;
                this.botMentionsBarChart.DataContext = mentionsCountChartData;
            }
            catch (ArgumentException ex)
            {
                ProgressSpinnerForMessagesPerBot.IsActive = false;
                ProgressSpinnerForMentionsPerBot.IsActive = false;

                await new MessageDialog(ex.Message, $"Statistics could not be loaded for \"{currentBot}\"").ShowAsync();
            }

            ProgressSpinnerForMessagesPerBot.IsActive = false;
            ProgressSpinnerForMentionsPerBot.IsActive = false;
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            DiscordianBotConsoleClient.StopAllBots();
        }
    }
}
