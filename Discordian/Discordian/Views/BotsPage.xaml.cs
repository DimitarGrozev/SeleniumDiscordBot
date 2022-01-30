using System;
using System.Linq;
using System.Collections.Generic;
using Discordian.Services;
using Discordian.ViewModels;
using Discordian.Core.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public List<string> Emails { get; set; }

        public BotsViewModel ViewModel { get; } = new BotsViewModel();
        public static Dictionary<Guid, bool> ActiveBots = new Dictionary<Guid, bool>();

        public BotsPage()
        {
            InitializeComponent();
            Loaded += BotsPage_Loaded;
        }

        private async void BotsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);

            foreach (var bot in ViewModel.SampleItems)
            {
                if (!ActiveBots.ContainsKey(bot.Id))
                    ActiveBots.Add(bot.Id, false);
            }
        }

        private async void ShowBotCreationDialog(object sender, RoutedEventArgs e)
        {
            var botCount = await DiscordianDbContext.GetBotCountAsync();

            if (botCount < 10)
            {
                var emails = await DiscordianDbContext.GetSavedEmailsAsync();
                var currentEmails = this.EmailTextBox.Items.Skip(1).ToList();

                foreach (var item in currentEmails)
                {
                    this.EmailTextBox.Items.Remove(item);
                }

                emails.ForEach(a => this.EmailTextBox.Items.Add(a));

                await AddBotContentDialog.ShowAsync();
            }
            else
            {
                await new MessageDialog("You can have a maximum of 10 bots at a time!", "Bot count exceeded").ShowAsync();
            }
        }

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            var id = this.BotIdTextBox.Text;
            var botName = this.BotNameTextBox.Text;
            var serverName = this.ServerNameTextBox.Text;
            var channelName = this.ChannelNameTextBox.Text;
            var messageDelay = this.MessageDelayNumberBox.Text;
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;
            var token = string.Empty;

            if (this.ValidateBotData(id, botName, serverName, channelName, messageDelay, email, password))
            {

                if (await DiscordianDbContext.AccountIsSavedAsync(email, password))
                {
                    token = await DiscordianDbContext.GetTokenForExistingAccountAsync(email, password);
                }
                else
                {
                    this.ProgressSpinner.IsActive = true;
                    this.AddBotContentDialog.Opacity = 0.5;

                    token = await DiscordianBotConsoleClient.GetTokenForNewAccountAsync(email, password);

                    this.ProgressSpinner.IsActive = false;
                    this.AddBotContentDialog.Opacity = 1;

                    if (string.IsNullOrEmpty(token))
                    {
                        BotCreationValidationMessage.Visibility = Visibility.Visible;
                        BotCreationValidationMessage.Text = "Wrong credentials! Try again!";
                        return;
                    }
                }

                try
                {
                    var discordData = await DiscordApiClient.GetDiscordDataForBot(serverName, channelName, token);

                    await DiscordianDbContext.AddDiscordDataToBotAsync(discordData, Guid.Parse(id));
                    await DiscordianDbContext.CreateBotAsync(id, botName, serverName, channelName, int.Parse(messageDelay), email, password, token);
                }
                catch (ArgumentException ex)
                {
                    BotCreationValidationMessage.Visibility = Visibility.Visible;
                    BotCreationValidationMessage.Text = ex.Message;
                    return;
                }

                AddBotContentDialog.Hide();

                this.BotIdTextBox.Text = string.Empty;
                this.BotNameTextBox.Text = string.Empty;
                this.ServerNameTextBox.Text = string.Empty;
                this.ChannelNameTextBox.Text = string.Empty;
                this.MessageDelayNumberBox.Text = "0";
                this.ChosenFileName.Text = string.Empty;
                this.EmailTextBox.Text = string.Empty;
                this.PasswordTextBox.Password = string.Empty;

                this.ProgressSpinner.IsActive = false;
                this.AddBotContentDialog.Opacity = 1;

                BotCreationValidationMessage.Visibility = Visibility.Collapsed;
                BotCreationValidationMessage.Text = string.Empty;

                ActiveBots[Guid.Parse(id)] = false;

                await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
            }
            else
            {
                BotCreationValidationMessage.Visibility = Visibility.Visible;
                BotCreationValidationMessage.Text = "Please fill out all of information!";
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();

            var id = this.BotIdTextBox.Text;
            await DiscordianDbContext.DeleteMessagesForBotAsync(id);

            this.BotIdTextBox.Text = string.Empty;
            this.BotNameTextBox.Text = string.Empty;
            this.ServerNameTextBox.Text = string.Empty;
            this.ChannelNameTextBox.Text = string.Empty;
            this.MessageDelayNumberBox.Text = "0";
            this.ChosenFileName.Text = string.Empty;
            this.EmailTextBox.Text = string.Empty;
            this.PasswordTextBox.Password = string.Empty;

            BotCreationValidationMessage.Visibility = Visibility.Collapsed;
            BotCreationValidationMessage.Text = string.Empty;
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var id = Guid.Parse((button).Tag.ToString());

            if (ActiveBots.GetValueOrDefault(id) == false)
            {
                var messages = await DiscordianDbContext.GetMessagesForBotAsync(id);
                var bot = await DiscordianDbContext.FindBotByIdAsync(id);

                await DiscordianBotConsoleClient.StartBotAsync(bot, messages);

                ActiveBots[id] = true;

                this.SwitchToStopIcon(button);
            }
            else
            {
                await DiscordianBotConsoleClient.StopBotAsync(id);

                ActiveBots[id] = false;

                this.SwitchToStartIcon(button);
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
                this.BotIdTextBox.Text = botId.ToString();
            }
        }

        private async void EmailTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var password = await DiscordianDbContext.GetPasswordForAccountAsync(EmailTextBox.Text);

            if (!string.IsNullOrEmpty(password))
            {
                this.PasswordTextBox.Password = password;
            }
        }

        private bool ValidateBotData(string id, string botName, string serverName, string channelName, string messageDelay, string email, string password)
        {
            var isIdValidGuid = Guid.TryParse(id, out Guid result);
            var isEmailValid = Regex.Match(email, "^[\\w.-]+@(?=[a-z\\d][^.]*\\.)[a-z\\d.-]*[^.]$").Success;
            var isBotNameValid = botName.Length <= 25;

            return isIdValidGuid && isEmailValid && isBotNameValid;
        }

        private void StartStopButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var id = Guid.Parse(button.Tag.ToString());
            if (ActiveBots[id])
            {
                this.SwitchToStopIcon(button);
            }
        }

        private void SwitchToStopIcon(Button button)
        {
            var fontIcon = new FontIcon();
            fontIcon.FontSize = 24;
            fontIcon.VerticalAlignment = VerticalAlignment.Center;
            fontIcon.Glyph = "\xE769";

            button.Content = fontIcon;
        }

        private void SwitchToStartIcon(Button button)
        {
            var fontIcon = new FontIcon();
            fontIcon.FontSize = 24;
            fontIcon.VerticalAlignment = VerticalAlignment.Center;
            fontIcon.Glyph = "\xE768";

            button.Content = fontIcon;
        }
    }
}
