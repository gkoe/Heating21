using Radzen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wasm.Helper
{
    public static class ExtensionMethods
    {
        public static void ShowNotification(this NotificationService notificationService,
                NotificationSeverity severity, string summary, params string[] details)
        {
            var message = new NotificationMessage
            {
                Style = "position: absolute; left: -1000px;",
                Severity = severity,
                Summary = summary,
                Detail = string.Join(',', details),
                Duration = 10000
            };
            notificationService.Notify(message);

            Console.WriteLine($"{message.Severity} notification");
        }

    }
}
