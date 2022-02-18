using System;
using Discordian.Services;
using Discordian.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Discordian.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, navigationView, KeyboardAccelerators);
            this.CheckSubscriptionStatus();
        }

        private void CheckSubscriptionStatus()
        {
            var subscription = LoginPage.userSubscription;

            //Check if user is with a promoted account and if not check his subscription status
            if (subscription?.IsPromotionSubscription == false)
            {
                if (subscription == null)
                {
                    this.InitialSubscriptionNotification.IsOpen = true;
                }

                if (subscription.UserSubscriptions == null)
                {
                    this.SubscriptionRenewalNotification.IsOpen = true;
                }
            }

            this.WelcomeBackMessage.Text = subscription.Email;
        }

        private async void NavigationViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync("Restart after login");
        }
    }
}
