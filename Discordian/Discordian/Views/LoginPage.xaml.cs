using System;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models.Wix;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.Web.Http;


namespace Discordian.Views
{
    public sealed partial class LoginPage : Page
    {
        private readonly string AuthorizeURL = "https://discordian.app/_functions/authorize";

        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginSpinner.IsActive = true;

            var isSubscribed = await this.CheckIfUserIsSubscribedAsync();

            LoginSpinner.IsActive = false;

            if (isSubscribed)
            {
                Window.Current.Content = new Lazy<UIElement>(CreateShell).Value ?? new Frame();
                Window.Current.Activate();
                Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

                return;
            }
            else
            {
                this.AccountValidationErrorMessage.Text = "Please renew your subscription!";
            }
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

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
