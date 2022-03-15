using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;

namespace Discordian.Services
{
    public static class BackgroundTaskSchedulerService
    {
        private static readonly string taskName = "SubscriptionCheckTask";
        private static WixApiClient wixApiClient = new WixApiClient();

        public static async Task ScheduleSubscriptionStatusCheckAsync()
        {
            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (requestStatus == BackgroundAccessStatus.DeniedBySystemPolicy || requestStatus == BackgroundAccessStatus.DeniedByUser)
            {
                //Show alert to enable background tasks
                return;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            var dailyTrigger = new TimeTrigger(1440, false);
            var builder = new BackgroundTaskBuilder();

            builder.Name = taskName;
            builder.SetTrigger(dailyTrigger);
            builder.Register();
        }

        public static async Task ChechSubscriptionStatusAsync()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var email = localSettings.Values["user"].ToString();
            var subscription = await wixApiClient.GetSubscriptionAsync(email);

            if (!subscription.IsPromotionSubscription && subscription.UserSubscriptions == null)
            {
                DiscordianBotConsoleClient.StopAllBots();
                CoreApplication.Exit();
            }
        }
    }
}
