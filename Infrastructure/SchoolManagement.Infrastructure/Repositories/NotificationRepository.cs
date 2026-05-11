using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(SchoolDbContext context) : base(context)
    {
    }
}
