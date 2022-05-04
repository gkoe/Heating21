using Microsoft.AspNetCore.Components;

using Radzen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wasm.Services
{
    public class UtilityServices
    {
        [Inject]
        NotificationService NotificationService { get; }

        public UtilityServices(NotificationService notificationService)
        {
            NotificationService = notificationService;
        }

        public void ShowNotification(NotificationSeverity severity, string summary, params string[] details)
        {
            var message = new NotificationMessage
            {
                Style = "position: absolute; left: -1000px;",
                Severity = severity,
                Summary = summary,
                Detail = string.Join(',', details),
                Duration = 10000
            };
            NotificationService.Notify(message);

            Console.WriteLine($"{message.Severity} notification");
        }
    }
}
