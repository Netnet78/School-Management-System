using Microsoft.EntityFrameworkCore;
using SchoolManagement.Infrastructure.Data;
using SchoolManagement.Infrastructure.Shared.Querying;
using SchoolManagement.Infrastructure.Shared.Repositories;
using System.Linq.Expressions;

namespace SchoolManagement.Infrastructure.Features.Candidates.Repositories;

public class CandidateRepository : BaseRepository<Candidate>, ICandidateRepository
{
    private static readonly Expression<Func<Candidate, bool>> HasAllRequiredData = c =>
        c.FirstName != null && c.FirstName != "" &&
        c.LatinFirstName != null && c.LatinFirstName != "" &&
        c.LastName != null && c.LastName != "" &&
        c.LatinLastName != null && c.LatinLastName != "" &&
        c.DateOfBirth != null &&
        c.Skill != null &&
        c.BirthVillage != null && c.BirthVillage != "" &&
        c.BirthCommune != null && c.BirthCommune != "" &&
        c.BirthDistrict != null && c.BirthDistrict != "" &&
        c.BirthProvince != null && c.BirthProvince != "" &&
        c.FatherName != null && c.FatherName != "" &&
        c.MotherName != null && c.MotherName != "" &&
        c.FatherOccupation != null && c.FatherOccupation != "" &&
        c.MotherOccupation != null && c.MotherOccupation != "" &&
        c.Religion != null && c.Religion != "" &&
        c.PhoneNumber != null && c.PhoneNumber != "" &&
        c.ExamCenter != null && c.ExamCenter != "" &&
        c.ExamDate != null &&
        c.ExamTable != null &&
        c.ExamRoom != null &&
        c.FromSchool != null && c.FromSchool != "";

    private static readonly Expression<Func<Candidate, bool>> HasPhoto = c =>
        c.Photo != null && c.Photo.Key != null && c.Photo.Key != "";

    public CandidateRepository(SchoolDbContext context) : base(context)
    {
    }

    protected override IQueryable<Candidate> CreateQuery()
    {
        return Set
            .AsNoTracking()
            .Include(c => c.Skill)
            .Include(c => c.Photo)
            .Include(c => c.Student)
            .Where(c => c.Student == null);
    }

    public async Task<IEnumerable<Candidate>> GetPagedAsync(int page, int pageSize,
        IEnumerable<FilterCondition<Candidate>>? filters,
        StudentDataStateFilterOptions dataState,
        string? sortBy,
        OrderDirection orderBy)
    {
        IQueryable<Candidate> query = BuildFilteredQuery(filters, dataState, sortBy, orderBy);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(
        IEnumerable<FilterCondition<Candidate>>? filters,
        StudentDataStateFilterOptions dataState)
    {
        IQueryable<Candidate> query = BuildFilteredQuery(filters, dataState, sortBy: null, orderBy: OrderDirection.Descending, applySorting: false);
        return await query.CountAsync();
    }

    private IQueryable<Candidate> BuildFilteredQuery(
        IEnumerable<FilterCondition<Candidate>>? filters,
        StudentDataStateFilterOptions dataState,
        string? sortBy,
        OrderDirection orderBy,
        bool applySorting = true)
    {
        IQueryable<Candidate> query = CreateQuery();

        if (filters != null)
        {
            query = query.ApplyFilters(filters);
        }

        query = ApplyDataStateFilter(query, dataState);

        if (applySorting)
        {
            query = ApplySorting(query, sortBy, orderBy);
        }

        return query;
    }

    private static IQueryable<Candidate> ApplyDataStateFilter(IQueryable<Candidate> query, StudentDataStateFilterOptions filterOption)
    {
        switch (filterOption)
        {
            case StudentDataStateFilterOptions.Completed:
                return query.Where(HasAllRequiredData).Where(HasPhoto);

            case StudentDataStateFilterOptions.MissingData:
            {
                IQueryable<int> completeIds = query.Where(HasAllRequiredData).Select(c => c.Id);
                return query.Where(HasPhoto).Where(c => !completeIds.Contains(c.Id));
            }

            case StudentDataStateFilterOptions.NoPicture:
            {
                IQueryable<int> withPhotoIds = query.Where(HasPhoto).Select(c => c.Id);
                return query.Where(c => !withPhotoIds.Contains(c.Id));
            }

            case StudentDataStateFilterOptions.MissingDataAndPicture:
            {
                IQueryable<int> completeWithPhotoIds = query.Where(HasAllRequiredData).Where(HasPhoto).Select(c => c.Id);
                return query.Where(c => !completeWithPhotoIds.Contains(c.Id));
            }

            default:
                return query;
        }
    }

    private static IQueryable<Candidate> ApplySorting(IQueryable<Candidate> query, string? sortBy, OrderDirection orderBy)
    {
        string key = sortBy?.Trim().ToLowerInvariant() ?? "";

        return key switch
        {
            "fullname" or "full_name" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName).ThenByDescending(x => x.Id),

            "latinfullname" or "latin_full_name" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.LatinLastName).ThenBy(x => x.LatinFirstName).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.LatinLastName).ThenByDescending(x => x.LatinFirstName).ThenByDescending(x => x.Id),

            "createdat" or "created" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),

            "dateofbirth" or "dob" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.DateOfBirth).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.DateOfBirth).ThenByDescending(x => x.Id),

            "examdate" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.ExamDate).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.ExamDate).ThenByDescending(x => x.Id),

            "skill" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.Skill.KhmerName).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Skill.KhmerName).ThenByDescending(x => x.Id),

            "gender" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.Gender).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.Gender).ThenByDescending(x => x.Id),

            "siblingscount" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.SiblingsCount).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.SiblingsCount).ThenByDescending(x => x.Id),

            "examtable" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.ExamTable).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.ExamTable).ThenByDescending(x => x.Id),

            "examroom" => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.ExamRoom).ThenBy(x => x.Id)
                : query.OrderByDescending(x => x.ExamRoom).ThenByDescending(x => x.Id),

            _ => orderBy == OrderDirection.Ascending
                ? query.OrderBy(x => x.Id)
                : query.OrderByDescending(x => x.Id),
        };
    }
}
