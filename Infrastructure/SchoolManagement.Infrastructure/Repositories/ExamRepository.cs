using SchoolManagement.Core.Infrastructure.Interfaces;
using SchoolManagement.Core.Models;
using SchoolManagement.Infrastructure.Data;

namespace SchoolManagement.Infrastructure.Repositories;

public class ExamRepository : BaseRepository<Exam>, IExamRepository
{
    public ExamRepository(SchoolDbContext context) : base(context)
    {
    }
}
