using Microsoft.EntityFrameworkCore;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{

    public class NotificationRepository : INotificationRepository
    {
        private readonly SchoolDbContext _context;

        public NotificationRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            return await _context.Notifications.ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task AddAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var existing = await _context.Notifications.FindAsync(notification.Id);
            if (existing == null)
            {
                _context.Notifications.Attach(notification);
                _context.Entry(notification).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(notification);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            ArgumentNullException.ThrowIfNull(notification);
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}