using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories
{
    public class EmployeePhotoRepository : BaseRepository<EmployeePhoto>, IEmployeePhotoRepository
    {
        private readonly SchoolDbContext _context;
        public EmployeePhotoRepository(SchoolDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeePhoto>> GetPendingDeletes(CancellationToken? token = null)
        {

            return await _context.EmployeePhotos.Where(p => p.FileStatus == FileStatus.PendingDelete).ToListAsync(cancellationToken: token ?? default);
        }

        public async Task<IEnumerable<EmployeePhoto>> GetPendingUploads(CancellationToken? token = null)
        {
            return await _context.EmployeePhotos
                .Where(p => p.FileStatus == FileStatus.PendingUpload)
                .ToListAsync(cancellationToken: token ?? default);
        }
    }
}
