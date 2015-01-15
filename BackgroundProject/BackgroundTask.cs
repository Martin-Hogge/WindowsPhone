using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BackgroundProject
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        //Nous avons bien compris le fonctionnement des Background Agents mais nous n'avons pas trouvé d'utilité à en avoir un.
        //Donc nous en avons fait pour montrer que nous savions le faire malgré que celui-ci ne fait que faire un Debug.WriteLine()

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");
            Debug.WriteLine("Le fuseau horaire à changer");
            deferral.Complete();
        }

    }
}
