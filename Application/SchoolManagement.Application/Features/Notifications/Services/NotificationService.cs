namespace SchoolManagement.Application.Features.Notifications.Services
{
    public class NotificationService : CrudServiceBase<Notification>, INotificationService
    {
        public NotificationService(INotificationRepository repository) : base(repository)
        {
        }
    }
}


