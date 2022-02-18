using Discordian.Core.Models.Wix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Discordian.Services
{
    public class BackgroundTaskSchedulerService
    {
        private readonly string entryPoint = "BackgroundTaskScheduler.SubscriptionStatusCheck";
        private readonly string taskName = "SubscriptionStatusCheck";

        public async Task ScheduleSubscriptionStatusCheckAsync()
        {
            var requestStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (requestStatus != BackgroundAccessStatus.AlwaysAllowed)
            {
                // Depending on the value of requestStatus, provide an appropriate response
                // such as notifying the user which functionality won't work as expected

                return;
            }

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == taskName)
                {
                    cur.Value.Unregister(true);
                }
            }

            var dailyTrigger = new TimeTrigger(10, false);

            var builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.TaskEntryPoint = entryPoint;
            builder.SetTrigger(dailyTrigger);
        }

        public static async Task SubscriptionStatusCheck()
        {

        }
    }
}
