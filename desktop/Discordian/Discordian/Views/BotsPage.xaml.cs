using System;
using Discordian.Core.Services;
using Discordian.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public string BotName { get; set; }
        public string ServerName { get; set; }
        public string ChannelName { get; set; }
        public int MessageDelay{ get; set; } 


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

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            await AddBotContentDialog.ShowAsync();
        }

        private async void addBotBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = this.BotName;
            var serverName = this.ServerName;
            var channelName = this.ChannelName;
            var messageDelay = this.MessageDelay;

            await DiscordianDbContext.CreateBotAsync(name, serverName, channelName, messageDelay);


            AddBotContentDialog.Hide();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();
        }
    }
}
