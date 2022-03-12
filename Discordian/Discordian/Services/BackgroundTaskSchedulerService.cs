using Discordian.Core.Models.Wix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace Discordian.Services
{
    public static class BackgroundTaskSchedulerService
    {
        private static readonly string taskName = "SubscriptionCheckTask";

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
    }
}
