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

            this.BotNameTextBox.Text = string.Empty;
            this.ServerNameTextBox.Text = string.Empty;
            this.ChannelNameTextBox.Text = string.Empty;
            this.MessageDelayNumberBox.Text = string.Empty;
            
            await ViewModel.LoadDataAsync(ListDetailsViewControl.ViewState);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddBotContentDialog.Hide();
        }

        private async void StartStopBot_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (!IsConsoleAppRunning)
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");

                button.Foreground = new SolidColorBrush(Colors.Green);
                ToolTipService.SetToolTip(button, "Terminate");
            }
            else
            {
                var valueSet = new ValueSet();
                valueSet.Add("terminate", true);
                button.Foreground = new SolidColorBrush(Colors.Red);
                var diagnosticAccessStatus = await AppDiagnosticInfo.RequestAccessAsync();
                ToolTipService.SetToolTip(button, "Launch");

                if (diagnosticAccessStatus == DiagnosticAccessStatus.Allowed)
                {
                    var processesInfo = ProcessDiagnosticInfo.GetForProcesses();

                    var processInfo = processesInfo.Where(x => x.ExecutableFileName == "ConsoleApp37.exe").FirstOrDefault();

                    if (processInfo != null)
                    {
                        var process =  Process.GetProcessesByName(processInfo.ExecutableFileName);
                        
                    }
                }
            }

            IsConsoleAppRunning = !IsConsoleAppRunning;
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());

            try
            {
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
                }
            }
            catch(ArgumentException ex)
            {
               //TODO: Implement notification if bot fails to send data
            }
        }
        private async void ShowDeleteBotFlyout_Click(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(sender as Button);
        }

        private async void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
        {
            var id = Guid.Parse((sender as Button).Tag.ToString());

            await DiscordianDbContext.DeleteBotById(id);

            Frame.Navigate(this.GetType());
        }
    }
}
