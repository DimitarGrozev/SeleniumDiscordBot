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
using Discordian.Core.Models.XAML;
using Windows.UI.Input.Preview.Injection;
using Windows.System;
using Discordian.Core.Models;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

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

        private async void ShowBotCreationDialog_Click(object sender, RoutedEventArgs e)
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

                this.cancelBtn.IsEnabled = true;
                await AddBotContentDialog.ShowAsync();
            }
            else
            {
                await new MessageDialog("You can have a maximum of 10 bots at a time!", "Bot count exceeded").ShowAsync();
            }
        }

        private async void CreateBotButton_Click(object sender, RoutedEventArgs e)
        {
            this.ProgressSpinner.IsActive = true;

            var botName = this.BotNameTextBox.Text;
            var serverName = this.ServerNameTextBox.SelectedItem?.ToString();
            var channelName = this.ChannelNameTextBox.SelectedItem?.ToString();
            var messageDelay = this.MessageDelayNumberBox.Text;
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;
            var chosenFileName = this.ChosenFileName.Text;
            var token = this.TokenTextBox.Text;

            var bot = new Bot();

            if (this.BotDetailsAreValid(botName, serverName, channelName, messageDelay, chosenFileName))
            {
                try
                {
                    var discordData = await DiscordApiClient.GetDiscordDataForBot(serverName, channelName, token);

                    bot = await DiscordianDbContext.CreateBotAsync(botName, serverName, channelName, int.Parse(messageDelay), email, password, token, chosenFileName, discordData);

                }
                catch (ArgumentException ex)
                {
                    this.ProgressSpinner.IsActive = false;

                    BotCreationValidationMessage.Visibility = Visibility.Visible;
                    BotCreationValidationMessage.Text = ex.Message;

                    return;
                }

                AddBotContentDialog.Hide();

                this.BotIdTextBox.Text = string.Empty;
                this.BotNameTextBox.Text = string.Empty;
                this.ServerNameTextBox.Items.Clear();
                this.ServerNameTextBox.Text = string.Empty;
                this.ChannelNameTextBox.Items.Clear();
                this.ChannelNameTextBox.Text = string.Empty;
                this.MessageDelayNumberBox.Text = "0";
                this.ChosenFileName.Text = string.Empty;
                this.EmailTextBox.Text = string.Empty;
                this.EmailTextBox.SelectedItem = string.Empty;
                this.PasswordTextBox.Password = string.Empty;
                this.TokenTextBox.Text = string.Empty;

                this.ProgressSpinner.IsActive = false;
                this.AddBotContentDialog.Opacity = 1;

                BotCreationValidationMessage.Visibility = Visibility.Collapsed;
                BotCreationValidationMessage.Text = string.Empty;

                this.AccountSelectionTab.IsSelected = true;
                this.AccountSelectionTab.IsEnabled = true;
                this.BotDetailsTab.IsEnabled = false;

                ActiveBots[bot.Id] = false;

                await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
            }
            else
            {
                this.ProgressSpinner.IsActive = false;

                BotCreationValidationMessage.Visibility = Visibility.Visible;
                BotCreationValidationMessage.Text = "Please fill out all of the information!";
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();

            this.EmailTextBox.Text = string.Empty;
            this.EmailTextBox.SelectedItem = string.Empty;
            this.PasswordTextBox.Password = string.Empty;
            this.BotIdTextBox.Text = string.Empty;
            this.BotNameTextBox.Text = string.Empty;
            this.ServerNameTextBox.Text = string.Empty;
            this.ChannelNameTextBox.Text = string.Empty;
            this.MessageDelayNumberBox.Text = "0";
            this.ChosenFileName.Text = string.Empty;
            this.TokenTextBox.Text = string.Empty;

            BotCreationValidationMessage.Visibility = Visibility.Collapsed;
            BotCreationValidationMessage.Text = string.Empty;

            BotAccountValidationMessage.Visibility = Visibility.Collapsed;
            BotAccountValidationMessage.Text = string.Empty;
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
                this.DisableBotOperations(button);
            }
            else
            {
                await DiscordianBotConsoleClient.StopBotAsync(id);

                ActiveBots[id] = false;

                this.SwitchToStartIcon(button);
                this.EnableBotOperations(button);
            }
        }

        private async void ShowEditDialog_Click(object sender, RoutedEventArgs e)
        {
            var id = (sender as Button).Tag.ToString();

            if (!string.IsNullOrEmpty(id))
            {
                var parsedId = Guid.Parse(id);
                var bot = await DiscordianDbContext.GetBotByIdAsync(parsedId);
                this.PopulateBotDataForEdit(bot);

                await EditBotContentDialog.ShowAsync();
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
                await DiscordianDbContext.CreateMessagesForBotAsync(file);
                this.ChosenFileName.Text = file.Name;
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

        private async void ChooseAccount_Click(object sender, RoutedEventArgs e)
        {
            AccountChoiceLoader.IsActive = true;
            BotAccountValidationMessage.Visibility = Visibility.Collapsed;
            BotAccountValidationMessage.Text = string.Empty;

            var token = string.Empty;
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;

            if (this.ValidateCredentials(email, password))
            {
                if (await DiscordianDbContext.AccountIsSavedAsync(email, password))
                {
                    token = await DiscordianDbContext.GetTokenForExistingAccountAsync(email, password);
                }
                else
                {

                    token = await DiscordianBotConsoleClient.GetTokenForNewAccountAsync(email, password);

                    if (string.IsNullOrEmpty(token))
                    {
                        AccountChoiceLoader.IsActive = false;
                        BotAccountValidationMessage.Visibility = Visibility.Visible;
                        BotAccountValidationMessage.Text = "Wrong credentials! Try again!";

                        return;
                    }
                }

                this.TokenTextBox.Text = token;

                var servers = await DiscordApiClient.GetServersForUserAsync(token);
                ServerNameTextBox.Items.Clear();
                servers.ForEach(s => ServerNameTextBox.Items.Add(new ComboboxItem { Text = s.Name, Value = s.Id }));

                this.BotDetailsTab.IsEnabled = true;
                this.BotDetailsTab.IsSelected = true;
                this.AccountSelectionTab.IsEnabled = false;

                this.AccountChoiceLoader.IsActive = false;
            }
            else
            {
                AccountChoiceLoader.IsActive = false;
                BotAccountValidationMessage.Visibility = Visibility.Visible;
                BotAccountValidationMessage.Text = "Wrong credentials! Try again!";
            }
        }

        private async void Server_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var server = (sender as ComboBox).SelectedItem as ComboboxItem;

            if (server != null)
            {
                var token = this.TokenTextBox.Text;

                var channels = await DiscordApiClient.GetChannelsInServerAsync(server.Value.ToString(), token);

                ChannelNameTextBox.Items.Clear();
                channels.ForEach(c =>
                {
                    if (c.Type == 0)
                    {
                        ChannelNameTextBox.Items.Add(c.Name);
                    }
                });
            }
        }

        private async void BackToAccount_Click(object sender, RoutedEventArgs e)
        {
            await DiscordianDbContext.DeleteMessagesForBotInCreationAsync();

            this.BotIdTextBox.Text = string.Empty;
            this.BotNameTextBox.Text = string.Empty;
            this.ServerNameTextBox.SelectedIndex = -1;
            this.ServerNameTextBox.Text = string.Empty;
            this.ChannelNameTextBox.SelectedIndex = -1;
            this.ChannelNameTextBox.Text = string.Empty;
            this.MessageDelayNumberBox.Text = "0";
            this.ChosenFileName.Text = string.Empty;
            this.TokenTextBox.Text = string.Empty;

            this.AccountSelectionTab.IsEnabled = true;
            this.AccountSelectionTab.IsSelected = true;
            this.BotDetailsTab.IsEnabled = false;
        }

        private async void EditBotButton_Click(object sender, RoutedEventArgs e)
        {
            var id = this.EditBotId.Text;
            var botName = this.EditBotNameTextBox.Text;
            var messageDelay = Convert.ToInt32(this.EditMessageDelayNumberBox.Value);
            var messagesFileName = this.EditChosenFileName.Text;

            await DiscordianDbContext.EditBotAsync(id, botName, messageDelay, messagesFileName);

            this.EditBotContentDialog.Hide();

            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }

        private async void CancelBotEdit_Click(object sender, RoutedEventArgs e)
        {
            this.EditBotId.Text = string.Empty;
            this.EditBotNameTextBox.Text = string.Empty;

            this.EditServerNameTextBox.SelectedItem = string.Empty;

            this.EditChannelNameTextBox.SelectedItem = string.Empty;

            this.EditMessageDelayNumberBox.Value = 0;
            this.EditChosenFileName.Text = string.Empty;

            this.EditBotContentDialog.Hide();

            await DiscordianDbContext.DeleteMessagesForBotInCreationAsync();
        }

        private void EnableBotOperations(Button button)
        {
            var parent = VisualTreeHelper.GetParent(button);

            var editButton = VisualTreeHelper.GetChild(parent, 2) as Button;
            editButton.IsEnabled = true;

            var deleteButton = VisualTreeHelper.GetChild(parent, 3) as Button;
            deleteButton.IsEnabled = true;
        }

        private void DisableBotOperations(Button button)
        {
            var parent = VisualTreeHelper.GetParent(button);

            var editButton = VisualTreeHelper.GetChild(parent, 2) as Button;
            editButton.IsEnabled = false;

            var deleteButton = VisualTreeHelper.GetChild(parent, 3) as Button;
            deleteButton.IsEnabled = false;
        }
        private bool ValidateCredentials(string email, string password)
        {
            var isEmailValid = Regex.Match(email, "^[\\w.-]+@(?=[a-z\\d][^.]*\\.)[a-z\\d.-]*[^.]$").Success;
            var isPasswordPresent = password.Length > 0;

            return isPasswordPresent && isEmailValid;
        }

        private bool BotDetailsAreValid(string botName, string serverName, string channelName, string messageDelay, string messagesFileName)
        {
            var isBotNameValid = botName.Length <= 25;
            var isServerNamePresent = !string.IsNullOrEmpty(serverName);
            var isChannelNamePresent = !string.IsNullOrEmpty(channelName);
            var isMessageDelayValid = double.TryParse(messageDelay, out double delay);
            var isMessagesFilePresent = messagesFileName.Length > 0;

            return isBotNameValid && isServerNamePresent && isChannelNamePresent && isMessageDelayValid && isMessagesFilePresent;
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

        private void StartStopButton_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var id = Guid.Parse(button.Tag.ToString());

            if (ActiveBots[id])
            {
                this.SwitchToStopIcon(button);
            }
        }

        private void PopulateBotDataForEdit(Bot bot)
        {
            this.EditBotId.Text = bot.Id.ToString();
            this.EditBotNameTextBox.Text = bot.Name;

            this.EditServerNameTextBox.PlaceholderText = bot.Server.Name;
            this.EditServerNameTextBox.Items.Clear();

            this.EditChannelNameTextBox.PlaceholderText = bot.Server.Channel;
            this.EditChannelNameTextBox.Items.Clear();

            this.EditMessageDelayNumberBox.Value = bot.MessageDelay;
            this.EditChosenFileName.Text = bot.MessagesFileName;
        }
    }
}
