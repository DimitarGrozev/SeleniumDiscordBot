using System;
using System.Threading.Tasks;
using Discordian.Core.Helpers;
using Discordian.Core.Models.Wix;
using Discordian.Services;
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
        private readonly WixApiClient wixApiClient;

        public LoginPage()
        {
            this.InitializeComponent();
            this.wixApiClient = new WixApiClient();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.Black;
            titleBar.ButtonBackgroundColor = Colors.Black;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            this.AccountValidationErrorMessage.Text = string.Empty;

            LoginSpinner.IsActive = true;

            if (string.IsNullOrEmpty(this.EmailTextBox.Text) || string.IsNullOrEmpty(this.PasswordTextBox.Password))
            {
                LoginSpinner.IsActive = false;

                this.AccountValidationErrorMessage.Text = "Please enter email and password!";

                return;
            }

            var authenticationResponse = await this.AuthenticateUserAsync();


            if (!string.IsNullOrEmpty(authenticationResponse.Error))
            {
                LoginSpinner.IsActive = false;

                this.AccountValidationErrorMessage.Text = authenticationResponse.Error;

                return;
            }

            UserContextService.Subscription = await this.GetUserSubscription();

            LoginSpinner.IsActive = false;

            Window.Current.Content = new Lazy<UIElement>(CreateShell).Value ?? new Frame();
            Window.Current.Activate();
            Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

            return;
        }

        private async Task<Subscription> GetUserSubscription()
        {
            var email = this.EmailTextBox.Text;
            var subscription = await this.wixApiClient.GetSubscriptionAsync(email);

            return subscription;
        }

        private async Task<AuthenticationResponse> AuthenticateUserAsync()
        {
            var email = this.EmailTextBox.Text;
            var password = this.PasswordTextBox.Password;
            var authenticationResponse = await this.wixApiClient.AuthenticateAsync(email, password);

            return authenticationResponse;
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
