using System;
using System.Collections.Generic;
using System.Linq;
using Discordian.Core.Models.Charts;
using Discordian.Services;
using Discordian.Utilities;
using Discordian.ViewModels;
using Windows.ApplicationModel;
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
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(App.Connection == null)
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");
            }

            ProgressSpinnerForMessagesPerBot.IsActive = true;
            ProgressSpinnerForMentionsPerBot.IsActive = true;

            var discordApiClient = new DiscordApiClient();
            var bots = await DiscordianDbContext.GetBotListAsync();
            var messageCountChartData = new List<Data>();
            var mentionsCountChartData = new List<Data>();

            foreach (var bot in bots)
            {
                var token = bot.Credentials.Token;
                var discordData = bot.DiscordData;

                if(discordData == null)
                {
                    discordData = await discordApiClient.GetDiscordDataForBot(bot);
                    await DiscordianDbContext.AppendDiscordDataToBotAsync(discordData, bot.Id);
                }

                var messageCount = await discordApiClient.GetBotMessageCountInChannelAsync(discordData, token);
                var mentionsCount = await discordApiClient.GetBotMentionsCountInChannelAsync(discordData, token);

                messageCountChartData.Add(new Data { Category = bot.Name, Value = messageCount, LabelProperty = messageCount.ToString() });
                mentionsCountChartData.Add(new Data { Category = bot.Name, Value = mentionsCount, LabelProperty = mentionsCount.ToString() });
            }

            this.barSeries.DataContext = messageCountChartData;
            this.botMentionsBarChart.DataContext = mentionsCountChartData;

            ProgressSpinnerForMessagesPerBot.IsActive = false;
            ProgressSpinnerForMentionsPerBot.IsActive = false;
        }

    }
}
