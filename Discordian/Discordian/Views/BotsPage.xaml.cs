using System;
using Discordian.Services;
using Discordian.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Discordian.Core.Helpers;
using System.Collections.Generic;
using Windows.ApplicationModel;
using System.Linq;

namespace Discordian.Views
{
    public sealed partial class BotsPage : Page
    {
        public List<string> Emails { get; set; }

        public BotsViewModel ViewModel { get; } = new BotsViewModel();

        public BotsPage()
        {
            InitializeComponent();
            Loaded += BotsPage_Loaded;
        }

        private async void BotsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");
        }

        private async void ShowBotCreationDialog(object sender, RoutedEventArgs e)
        {
            var emails = await DiscordianDbContext.GetSavedEmailsAsync();
            var currentEmails = this.EmailTextBox.Items.Skip(1);

            foreach (var item in currentEmails)
            {
                this.EmailTextBox.Items.Remove(item);
            }

            emails.ForEach(a => this.EmailTextBox.Items.Add(a));

            await AddBotContentDialog.ShowAsync();
        }

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            var id = this.BotIdTextBox.Text;
            var botName = this.BotNameTextBox.Text;
            var serverName = this.ServerNameTextBox.Text;
            var channelName = this.ChannelNameTextBox.Text;
            var messageDelay = int.Parse(this.MessageDelayNumberBox.Text);
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;

            if (this.InformationIsPresent(id, botName, serverName, channelName, messageDelay, email, password))
            {
                if (await DiscordianDbContext.AccountIsSavedAsync(email, password))
                {
                    var token = await DiscordianDbContext.GetTokenForAccountAsync(email, password);
                    await DiscordianDbContext.CreateBotAsync(id, botName, serverName, channelName, messageDelay, email, password, token);
                }
                else
                {
                    this.ProgressSpinner.IsActive = true;
                    this.AddBotContentDialog.Opacity = 0.5;

                    var valueSet = new ValueSet();
                    valueSet.Add("request", "login");
                    valueSet.Add("email", email);
                    valueSet.Add("password", password);

                    if (App.Connection != null)
                    {
                        var response = await App.Connection.SendMessageAsync(valueSet);
                        var token = response.Message["token"].ToString().Replace("\"","");

                        this.ProgressSpinner.IsActive = false;
                        this.AddBotContentDialog.Opacity = 1;

                        if (!string.IsNullOrEmpty(token))
                        {
                            await DiscordianDbContext.CreateBotAsync(id, botName, serverName, channelName, messageDelay, email, password, token);
                            await DiscordianDbContext.SaveAccountAsync(email, password, token);
                        }
                        else
                        {
                            BotCreationValidationMessage.Visibility = Visibility.Visible;
                            BotCreationValidationMessage.Text = "Wrong credentials! Try again!";
                            return;
                        }
                    }
                }

                AddBotContentDialog.Hide();

                this.BotIdTextBox.Text = string.Empty;
                this.BotNameTextBox.Text = string.Empty;
                this.ServerNameTextBox.Text = string.Empty;
                this.ChannelNameTextBox.Text = string.Empty;
                this.MessageDelayNumberBox.Text = string.Empty;
                this.ChosenFileName.Text = string.Empty;
                this.EmailTextBox.Text = string.Empty;
                this.PasswordTextBox.Password = string.Empty;

                this.ProgressSpinner.IsActive = false;
                this.AddBotContentDialog.Opacity = 1;

                BotCreationValidationMessage.Visibility = Visibility.Collapsed;
                BotCreationValidationMessage.Text = string.Empty;

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
            this.MessageDelayNumberBox.Text = string.Empty;
            this.ChosenFileName.Text = string.Empty;
            this.EmailTextBox.Text = string.Empty;
            this.PasswordTextBox.Password = string.Empty;

            BotCreationValidationMessage.Visibility = Visibility.Collapsed;
            BotCreationValidationMessage.Text = string.Empty;
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());

            try
            {
                var messages = await DiscordianDbContext.GetMessagesForBotAsync(id);
                var bot = await DiscordianDbContext.FindBotByIdAsync(id);

                var valueSet = new ValueSet();
                valueSet.Add("request", "start");
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

        private bool InformationIsPresent(string id, string botName, string serverName, string channelName, int messageDelay, string email, string password)
        {
            return id != null && serverName != null && channelName != null && messageDelay != 0 && email != null && password != null;
        }
    }
}
