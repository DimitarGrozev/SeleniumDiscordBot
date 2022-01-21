using Discordian.Core.Helpers;
using Discordian.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Discordian.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            Loaded += LoginPage_Loaded;
        }

        private async void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Id");

            ProgressSpinner.IsActive = true;
            LoginForm.Opacity = 0.5;

            if (await DiscordianDbContext.IsAuthenticatedAsync())
            {
                Window.Current.Content = new Lazy<UIElement>(CreateShell).Value ?? new Frame();
                Window.Current.Activate();
                Frame.Navigate(typeof(BotsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

                return;
            }

            LoginForm.Opacity = 1;
            ProgressSpinner.IsActive = false;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            WrongCredentialsWarning.Visibility = Visibility.Collapsed;
            ProgressSpinner.IsActive = true;
            LoginForm.Opacity = 0.5;

            var valueSet = new ValueSet();
            valueSet.Add("request", "login");
            valueSet.Add("email", this.Email.Text);
            valueSet.Add("password", this.Password.Password);

            if (App.Connection != null)
            {
                var response = await App.Connection.SendMessageAsync(valueSet);
                var token = response.Message["token"].ToString();

                if (!string.IsNullOrEmpty(token))
                {
                    Window.Current.Content = new Lazy<UIElement>(CreateShell).Value ?? new Frame();
                    Window.Current.Activate();
                    Frame.Navigate(typeof(BotsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });

                    var credentials = await DiscordianDbContext.AuthenticateAsync(this.Email.Text, this.Password.Password, token);

                    return;
                }
            }

            LoginForm.Opacity = 1;
            ProgressSpinner.IsActive = false;
            WrongCredentialsWarning.Visibility = Visibility.Visible;
        }

        private UIElement CreateShell()
        {
            return new Views.ShellPage();
        }
    }
}
