using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models.Charts;
using Discordian.Core.Models.Wix;
using Discordian.Services;
using Discordian.Utilities;
using Discordian.ViewModels;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace Discordian.Views
{
    public sealed partial class MainPage : Page
    {
        private readonly string AuthorizeURL = "https://dgrozev99.wixsite.com/my-site/_functions-dev/authorize";
        private static bool isLoggedIn = false;
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                await this.LoginContentDialog.ShowAsync();
            }
            else
            {
                await this.LoadStatisticsDataAsync();
            }
        }

        private void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            DiscordianBotConsoleClient.StopAllBots();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginSpinner.IsActive = true;

            var isSubscribed = await this.CheckIfUserIsSubscribedAsync();

            LoginSpinner.IsActive = false;

            if (isSubscribed)
            {
                await this.LoadStatisticsDataAsync();

                isLoggedIn = true;
            }
            else
            {
                this.AccountValidationErrorMessage.Text = "Please renew your subscription!";
            }
        }

        private async Task LoadStatisticsDataAsync()
        {
            this.LoginContentDialog.Hide();

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

        private async Task<bool> CheckIfUserIsSubscribedAsync()
        {
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;

            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");
                var httpRequestMessage = new HttpRequestMessage();
                var content = new HttpStringContent(await Json.StringifyAsync(new User { email = email, password = password }), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                var url = new Uri(AuthorizeURL);

                var responseMessage = await httpClient.PostAsync(url, content);
                var responseBody = await responseMessage.Content.ReadAsStringAsync();
                var data = await Json.ToObjectAsync<AuthenticationResponse>(responseBody);

                return data.IsSubscribed;
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
