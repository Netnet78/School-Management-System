using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Repositories;

namespace SchoolManagement.Infrastructure.Features.Notifications.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(SchoolDbContext context) : base(context)
    {
    }
}
