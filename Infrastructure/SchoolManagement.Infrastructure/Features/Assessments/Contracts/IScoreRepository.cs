using SchoolManagement.Core.Features.Assessments.Models;
using SchoolManagement.Infrastructure.Shared.Contracts;

namespace SchoolManagement.Infrastructure.Features.Assessments.Contracts
{
    public interface IScoreRepository : IBaseRepository<Score>
    {
    }
}
