using Radzen;
using RadzenNotificationService = Radzen.NotificationService;

namespace MemoDown.Services
{
    public class NotificationService
    {
        private readonly RadzenNotificationService _notificationService;
        public NotificationService(RadzenNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Notify(NotificationSeverity severity, string detail, string summary = "", int duration = 3000)
        {
            _notificationService.Notify(new NotificationMessage
            {
                Style = "position: absolute; top: calc(100vh - 210px); right: calc(50vw - 125px);",
                Severity = severity,
                Summary = summary,
                Detail = detail,
                Duration = duration
            });
        }
    }
}
