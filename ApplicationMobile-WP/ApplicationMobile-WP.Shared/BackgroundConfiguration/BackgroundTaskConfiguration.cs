using ApplicationMobile_WP.DataAccess.RemoteModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace ApplicationMobile_WP.BackgroundConfiguration
{
    public class BackgroundTaskConfiguration
    {
        public const string EntryPoint = "BackgroundProject.BackgroundTask";
        public const string TimeTriggeredTaskName = "TimeTriggeredTask";
        public static bool TimeTriggeredTaskRegistered = false;
        public static string TimeTriggeredTaskProgress = "";
        public static List<DataModelMatch> matchToInsert = new List<DataModelMatch>();

        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            if (TaskRequiresBackgroundAccess(name))
            {
                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
            }

            BackgroundTaskRegistration task = builder.Register();

            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }

        public static bool TaskRequiresBackgroundAccess(String name)
        {
            #if WINDOWS_PHONE_APP
                return true;
            #else
                if (name == TimeTriggeredTaskName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            #endif
        }

        public static void UpdateBackgroundTaskStatus(String name, bool registered)
        {
            switch (name)
            {
                case TimeTriggeredTaskName:
                    TimeTriggeredTaskRegistered = registered;
                    break;
            }
        }
    }
}
