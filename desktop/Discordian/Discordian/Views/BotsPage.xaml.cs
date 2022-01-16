using System;
using Discordian.Services;
using Discordian.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using Discordian.Core.Helpers;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Diagnostics;
using System.Linq;
using Windows.System;
using Windows.System.Diagnostics;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public string BotName { get; set; }
        public string ServerName { get; set; }
        public string ChannelName { get; set; }
        public int MessageDelay { get; set; }

        private static bool IsConsoleAppRunning { get; set; }


        public BotsViewModel ViewModel { get; } = new BotsViewModel();

        public BotsPage()
        {
            InitializeComponent();
            Loaded += BotsPage_Loaded;
        }

        private async void BotsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }

        private async void ShowBotCreationDialog(object sender, RoutedEventArgs e)
        {
            await AddBotContentDialog.ShowAsync();
        }

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            var name = this.BotName;
            var serverName = this.ServerName;
            var channelName = this.ChannelName;
            var messageDelay = this.MessageDelay;

            await DiscordianDbContext.CreateBotAsync(name, serverName, channelName, messageDelay);

            AddBotContentDialog.Hide();

            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();
        }

        private async void StartStopBot_Click(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);

            if (!IsConsoleAppRunning)
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");

                button.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                var valueSet = new ValueSet();
                valueSet.Add("terminate", true);
                button.Foreground = new SolidColorBrush(Colors.Red);
            }

            IsConsoleAppRunning = !IsConsoleAppRunning;
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());
            var credentials = await DiscordianDbContext.GetCredentialsAsync();
            var appSettings = await DiscordianDbContext.GetAppSettingsAsync();
            var messages = await DiscordianDbContext.GetAllMessagesAsync();
            var bot = await DiscordianDbContext.FindBotByIdAsync(id);


            ValueSet valueSet = new ValueSet();
            valueSet.Add("credentials", await Json.StringifyAsync(credentials));
            valueSet.Add("appSettings", await Json.StringifyAsync(appSettings));
            valueSet.Add("messages", await Json.StringifyAsync(messages));
            valueSet.Add("bot", await Json.StringifyAsync(bot));

            if (App.Connection != null)
            {
                var response = await App.Connection.SendMessageAsync(valueSet);
                //var text = "Received response: " + response.Message["response"] as string;
            }
        }
    }
}
