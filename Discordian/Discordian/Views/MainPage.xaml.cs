using System;
using System.Collections.Generic;
using System.Linq;
using Discordian.Core.Models.Charts;
using Discordian.Services;
using Discordian.Utilities;
using Discordian.ViewModels;
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
            ProgressSpinnerForMessagesPerBot.IsActive = true;
            ProgressSpinnerForMentionsPerBot.IsActive = true;

            var discordApiClient = new DiscordApiClient();
            var bots = await DiscordianDbContext.GetBotListAsync();
            var messageCountChartData = new List<Data>();
            var mentionsCountChartData = new List<Data>();

            foreach (var bot in bots)
            {
                var token = bot.Credentials.Token;
                var messageCount = await discordApiClient.GetBotMessageCountInChannelAsync(bot.Server.Name, bot.Server.Channel, token);
                var mentionsCount = await discordApiClient.GetBotMentionsCountInChannelAsync(bot.Server.Name, bot.Server.Channel, token);

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
