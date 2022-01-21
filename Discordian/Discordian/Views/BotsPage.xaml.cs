using System;
using Discordian.Services;
using Discordian.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Discordian.Core.Helpers;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.System;
using Windows.System.Diagnostics;
using System.Linq;
using Discordian.Utilities;
using Windows.UI.Xaml.Controls.Primitives;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public string BotName { get; set; }

        public string BotId { get; set; }

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
            var id = this.BotId;
            var name = this.BotName;
            var serverName = this.ServerName;
            var channelName = this.ChannelName;
            var messageDelay = this.MessageDelay;

            if (!string.IsNullOrEmpty(id))
            {
                await DiscordianDbContext.CreateBotAsync(id, name, serverName, channelName, messageDelay);

                AddBotContentDialog.Hide();

                this.BotNameTextBox.Text = string.Empty;
                this.ServerNameTextBox.Text = string.Empty;
                this.ChannelNameTextBox.Text = string.Empty;
                this.MessageDelayNumberBox.Text = string.Empty;
                this.ChosenFileName.Text = string.Empty;

                await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();

            await DiscordianDbContext.DeleteMessagesForBotAsync(this.BotId);

            this.BotIdTextBox.Text = string.Empty;
            this.BotNameTextBox.Text = string.Empty;
            this.ServerNameTextBox.Text = string.Empty;
            this.ChannelNameTextBox.Text = string.Empty;
            this.MessageDelayNumberBox.Text = string.Empty;
            this.ChosenFileName.Text = string.Empty;
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());

            try
            {
                var credentials = await DiscordianDbContext.GetCredentialsAsync();
                var appSettings = await DiscordianDbContext.GetAppSettingsAsync();
                var messages = await DiscordianDbContext.GetMessagesForBotAsync(id);
                var bot = await DiscordianDbContext.FindBotByIdAsync(id);


                ValueSet valueSet = new ValueSet();
                valueSet.Add("request", "start");
                valueSet.Add("credentials", await Json.StringifyAsync(credentials));
                valueSet.Add("appSettings", await Json.StringifyAsync(appSettings));
                valueSet.Add("messages", await Json.StringifyAsync(messages));
                valueSet.Add("bot", await Json.StringifyAsync(bot));

                if (App.Connection != null)
                {
                    var response = await App.Connection.SendMessageAsync(valueSet);
                }
            }
            catch (ArgumentException ex)
            {
                //TODO: Implement notification if bot fails to send data
            }
        }

        private void ShowDeleteBotFlyout_Click(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as Button);
        }

        private async void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());

            await DiscordianDbContext.DeleteBotById(id);

            Frame.Navigate(this.GetType());
        }

        private async void ChooseMessages_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".txt");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var botId = await DiscordianDbContext.SaveMessagesForBotAsync(file);
                this.ChosenFileName.Text = file.Name;
                this.BotId = botId.ToString();
            }
        }
    }
}
