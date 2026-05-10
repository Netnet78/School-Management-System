using Microsoft.EntityFrameworkCore;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

namespace School_Management.Infrastructure.Repositories
{
    public class StudentPhotoRepository : BaseRepository<StudentPhoto>, IStudentPhotoRepository
    {
        private readonly SchoolDbContext _context;
        public StudentPhotoRepository(SchoolDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentPhoto>> GetPendingDeletes(CancellationToken? token = null)
        {

            return await _context.StudentPhotos.Where(p => p.FileStatus == FileStatus.PendingDelete).ToListAsync(cancellationToken: token ?? default);
        }

        public async Task<IEnumerable<StudentPhoto>> GetPendingUploads(CancellationToken? token = null)
        {
            return await _context.StudentPhotos
                .Where(p => p.FileStatus == FileStatus.PendingUpload)
                .ToListAsync(cancellationToken: token ?? default);
        }
    }
}
