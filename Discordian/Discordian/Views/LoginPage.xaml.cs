using System;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models.Wix;
using Discordian.Utilities;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Discordian.Views
{
    public sealed partial class LoginPage : Page
    {
        public static Subscription userSubscription;

        private readonly AppConfig appConfig = new AppConfig();
        private readonly string AuthenticateURL = "https://discordian.app/_functions/authenticate";

        public LoginPage()
        {
            this.InitializeComponent();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ButtonBackgroundColor = Colors.Black;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.AccountValidationErrorMessage.Text = string.Empty;

            LoginSpinner.IsActive = true;

            try
            {
                await this.AuthenticateUserAsync();

                LoginSpinner.IsActive = false;

                Window.Current.Content = new Lazy<UIElement>(CreateShell).Value ?? new Frame();
                Window.Current.Activate();
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

                return;
            }
            catch (Exception ex)
            {
                LoginSpinner.IsActive = false;

                this.AccountValidationErrorMessage.Text = "Invalid email or password!";
            }
        }

        private async Task AuthenticateUserAsync()
        {
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36");

            var content = new HttpStringContent(await Json.StringifyAsync(new User { email = email, password = password }), Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri(AuthenticateURL);
            httpRequestMessage.Headers.Add("discordian-api-key", this.appConfig.ApiKey.DiscordianApiKey);
            httpRequestMessage.Content = content;
            httpRequestMessage.Method = HttpMethod.Post;

            var responseMessage = await httpClient.SendRequestAsync(httpRequestMessage);

            if (responseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ArgumentException("Invalid username or password!");
            }

            if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                return;
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var authenticationResponse = await Json.ToObjectAsync<AuthenticationResponse>(responseContent);

            if (authenticationResponse?.Subscription != null)
            {
                userSubscription = authenticationResponse.Subscription;
            }
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
