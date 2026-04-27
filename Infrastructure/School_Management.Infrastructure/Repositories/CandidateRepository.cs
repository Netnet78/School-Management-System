using Microsoft.EntityFrameworkCore;
using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly SchoolDbContext _context;

        private static readonly Expression<Func<Candidate, bool>> CandidateHasCompleteDataPredicate = c =>
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

        public CandidateRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Candidate>> GetAllAsync()
        {
            return await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .OrderBy(c => c.Id)
                .ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetAllAsync(int? lastId, int pageSize)
        {
            IQueryable<Candidate> query = _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .OrderBy(c => c.Id)
                .AsQueryable()
                .WhereIf(lastId.HasValue, c => c.Id > lastId!.Value);

            return await query.Take(pageSize).ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesOnlyAsync(int? lastId, int pageSize)
        {
            IQueryable<Candidate> query = _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .Where(c => c.Student == null)
                .OrderBy(c => c.Id)
                .AsQueryable()
                .WhereIf(lastId.HasValue, c => c.Id > lastId!.Value);

            return await query.Take(pageSize).ToListAsync();
        }

        public async Task<Candidate?> GetByIdAsync(int id)
        {
            return await _context.Candidates.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            await _context.Candidates.AddAsync(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            Candidate? existing = await _context.Candidates.FindAsync(candidate.Id);
            if (existing == null)
            {
                _context.Candidates.Attach(candidate);
                _context.Entry(candidate).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(candidate);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Candidate candidate)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCandidatesOnlyCountAsync()
        {
            return await _context.Candidates.Where(c => c.Student == null).CountAsync();
        }

        public async Task<int> GetAllCountAsync()
        {
            return await _context.Candidates.CountAsync();
        }

        public async Task<IEnumerable<Candidate>> GetAllPagedAsync(int page, int pageSize)
        {
            return await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesOnlyPagedAsync(int page, int pageSize)
        {
            return await _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .Where(c => c.Student == null)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetAllPagedAsync(int page, int pageSize, StudentFilterOptions options)
        {
            IQueryable<Candidate> query = BuildFilteredQuery(candidatesOnly: false, options);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAllCountAsync(StudentFilterOptions options)
        {
            IQueryable<Candidate> query = BuildFilteredQuery(candidatesOnly: false, options, applySorting: false);
            return await query.CountAsync();
        }

        public async Task<int> GetCandidatesOnlyCountAsync(StudentFilterOptions options)
        {
            IQueryable<Candidate> query = BuildFilteredQuery(candidatesOnly: true, options, applySorting: false);
            return await query.CountAsync();
        }

        public async Task<IEnumerable<Candidate>> FindAsync(
            Expression<Func<Candidate, bool>> predicate,
            int? page,
            int pageSize,
            Func<IQueryable<Candidate>, IOrderedQueryable<Candidate>>? orderBy = null,
            params Expression<Func<Candidate, object>>[] includes)
        {
            IQueryable<Candidate> query = _context.Candidates.AsNoTracking();

            foreach (Expression<Func<Candidate, object>> include in includes)
            {
                query = query.Include(include);
            }

            query = query.Where(predicate);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query
                .Skip((pageSize * (page - 1)) ?? pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Candidate>> GetCandidatesOnlyPagedAsync(int page, int pageSize, StudentFilterOptions options)
        {
            IQueryable<Candidate> query = BuildFilteredQuery(candidatesOnly: true, options);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private IQueryable<Candidate> BuildFilteredQuery(bool candidatesOnly, StudentFilterOptions? options, bool applySorting = true)
        {
            options ??= new StudentFilterOptions();

            IQueryable<Candidate> query = _context.Candidates
                .AsNoTracking()
                .Include(c => c.Skill)
                .AsQueryable()
                .WhereIf(candidatesOnly, c => c.Student == null);

            query = ApplyOptionsFilter(query, options);
            query = ApplySearchFilter(query, options.Search, options.SearchField);
            query = ApplyDataStateFilter(query, options.DataState);

            if (applySorting)
            {
                query = ApplySorting(query, options);
            }

            return query;
        }

        private static IQueryable<Candidate> ApplyOptionsFilter(IQueryable<Candidate> query, StudentFilterOptions options)
        {
            if (options.FromDate.HasValue)
            {
                DateTime start = options.FromDate.Value.Date;
                query = query.Where(c => c.CreatedAt >= start);
            }

            if (options.ToDate.HasValue)
            {
                DateTime endExclusive = options.ToDate.Value.Date.AddDays(1);
                query = query.Where(c => c.CreatedAt < endExclusive);
            }

            if (options.IsActive.HasValue)
            {
                bool isActive = options.IsActive.Value;
                query = query.Where(c => c.Skill != null && c.Skill.IsActive == isActive);
            }

            if (TryParseGender(options.Gender, out Gender gender))
            {
                query = query.Where(c => c.Gender == gender);
            }

            return query;
        }

        private static IQueryable<Candidate> ApplySearchFilter(IQueryable<Candidate> query, string? search, StudentField? searchField)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return query;
            }

            string searchKey = search.Trim();
            string pattern = $"%{searchKey}%";
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            if (searchField == null)
            {
                bool parsedId = int.TryParse(searchKey, out int idValue);
                bool parsedGender = TryParseGender(searchKey, out Gender genderValue);

                return query.Where(x =>
                    EF.Functions.ILike(x.FirstName, pattern) ||
                    EF.Functions.ILike(x.LastName, pattern) ||
                    EF.Functions.ILike(x.LatinFirstName, pattern) ||
                    EF.Functions.ILike(x.LatinLastName, pattern) ||
                    EF.Functions.ILike(x.PhoneNumber, pattern) ||
                    EF.Functions.ILike(x.FromSchool, pattern) ||
                    EF.Functions.ILike(x.BirthProvince, pattern) ||
                    EF.Functions.ILike(x.BirthDistrict, pattern) ||
                    EF.Functions.ILike(x.BirthCommune, pattern) ||
                    EF.Functions.ILike(x.BirthVillage, pattern) ||
                    EF.Functions.ILike(x.FatherName, pattern) ||
                    EF.Functions.ILike(x.MotherName, pattern) ||
                    (x.Skill != null && EF.Functions.ILike(x.Skill.KhmerName, pattern)) ||
                    (parsedId && x.Id == idValue) ||
                    (parsedGender && x.Gender == genderValue));
            }

            if (searchField == StudentField.Age)
            {
                if (!int.TryParse(searchKey, out int ageValue))
                {
                    return query.Where(_ => false);
                }

                DateOnly minDob = today.AddYears(-(ageValue + 1)).AddDays(1);
                DateOnly maxDob = today.AddYears(-ageValue);

                return query.Where(x =>
                    x.DateOfBirth.HasValue &&
                    x.DateOfBirth.Value >= minDob &&
                    x.DateOfBirth.Value <= maxDob);
            }

            return searchField switch
            {
                StudentField.Id => int.TryParse(searchKey, out int idValue)
                    ? query.Where(x => x.Id == idValue)
                    : query.Where(_ => false),

                StudentField.FullName =>
                    query.Where(x =>
                        EF.Functions.ILike(x.FirstName, pattern) ||
                        EF.Functions.ILike(x.LastName, pattern)),

                StudentField.LatinFullName =>
                    query.Where(x =>
                        EF.Functions.ILike(x.LatinFirstName, pattern) ||
                        EF.Functions.ILike(x.LatinLastName, pattern)),

                StudentField.Gender => TryParseGender(searchKey, out Gender genderValue)
                    ? query.Where(x => x.Gender == genderValue)
                    : query.Where(_ => false),

                StudentField.Skill =>
                    query.Where(x => x.Skill != null && EF.Functions.ILike(x.Skill.KhmerName, pattern)),

                StudentField.BirthVillage =>
                    query.Where(x => EF.Functions.ILike(x.BirthVillage, pattern)),

                StudentField.BirthCommune =>
                    query.Where(x => EF.Functions.ILike(x.BirthCommune, pattern)),

                StudentField.BirthDistrict =>
                    query.Where(x => EF.Functions.ILike(x.BirthDistrict, pattern)),

                StudentField.BirthProvince =>
                    query.Where(x => EF.Functions.ILike(x.BirthProvince, pattern)),

                StudentField.FatherName =>
                    query.Where(x => EF.Functions.ILike(x.FatherName, pattern)),

                StudentField.MotherName =>
                    query.Where(x => EF.Functions.ILike(x.MotherName, pattern)),

                StudentField.FatherOccupation =>
                    query.Where(x => EF.Functions.ILike(x.FatherOccupation, pattern)),

                StudentField.MotherOccupation =>
                    query.Where(x => EF.Functions.ILike(x.MotherOccupation, pattern)),

                StudentField.SiblingsCount => int.TryParse(searchKey, out int siblingsCountValue)
                    ? query.Where(x => x.SiblingsCount == siblingsCountValue)
                    : query.Where(_ => false),

                StudentField.Religion =>
                    query.Where(x => EF.Functions.ILike(x.Religion, pattern)),

                StudentField.PhoneNumber =>
                    query.Where(x => EF.Functions.ILike(x.PhoneNumber, pattern)),

                StudentField.ExamCenter =>
                    query.Where(x => EF.Functions.ILike(x.ExamCenter, pattern)),

                StudentField.ExamTable => int.TryParse(searchKey, out int examTableValue)
                    ? query.Where(x => x.ExamTable.HasValue && x.ExamTable.Value == examTableValue)
                    : query.Where(_ => false),

                StudentField.ExamRoom => int.TryParse(searchKey, out int examRoomValue)
                    ? query.Where(x => x.ExamRoom.HasValue && x.ExamRoom.Value == examRoomValue)
                    : query.Where(_ => false),

                StudentField.FromSchool =>
                    query.Where(x => EF.Functions.ILike(x.FromSchool, pattern)),

                StudentField.OtherInfo =>
                    query.Where(x => EF.Functions.ILike(x.OtherInfo, pattern)),

                StudentField.CreatedAt => DateTime.TryParse(searchKey, out DateTime createdAtDate)
                    ? query.Where(x => x.CreatedAt.Date == createdAtDate.Date)
                    : query.Where(_ => false),

                StudentField.DateOfBirth => DateOnly.TryParse(searchKey, out DateOnly dobDate)
                    ? query.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth.Value == dobDate)
                    : query.Where(_ => false),

                StudentField.ExamDate => DateOnly.TryParse(searchKey, out DateOnly examDate)
                    ? query.Where(x => x.ExamDate.HasValue && x.ExamDate.Value == examDate)
                    : query.Where(_ => false),

                _ => query
            };
        }

        private static IQueryable<Candidate> ApplyDataStateFilter(IQueryable<Candidate> query, StudentDataStateFilterOptions filterOption)
        {
            return filterOption switch
            {
                StudentDataStateFilterOptions.Completed =>
                    query.Where(CandidateHasCompleteDataPredicate)
                        .Where(x => x.PhotoKey != null && x.PhotoKey != ""),

                StudentDataStateFilterOptions.MissingData =>
                    query.Where(Not(CandidateHasCompleteDataPredicate))
                        .Where(x => x.PhotoKey != null && x.PhotoKey != ""),

                StudentDataStateFilterOptions.NoPicture =>
                    query.Where(x => x.PhotoKey == null || x.PhotoKey == ""),

                StudentDataStateFilterOptions.MissingDataAndPicture =>
                    query.Where(Or(Not(CandidateHasCompleteDataPredicate), x => x.PhotoKey == null || x.PhotoKey == "")),

                _ => query
            };
        }

        private static IQueryable<Candidate> ApplySorting(IQueryable<Candidate> query, StudentFilterOptions options)
        {
            StudentField sortField = ResolveSortField(options.SortBy);

            return sortField switch
            {
                StudentField.FullName => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName).ThenByDescending(x => x.Id),

                StudentField.LatinFullName => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.LatinLastName).ThenBy(x => x.LatinFirstName).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.LatinLastName).ThenByDescending(x => x.LatinFirstName).ThenByDescending(x => x.Id),

                StudentField.CreatedAt => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id),

                StudentField.DateOfBirth => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.DateOfBirth).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.DateOfBirth).ThenByDescending(x => x.Id),

                StudentField.ExamDate => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.ExamDate).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.ExamDate).ThenByDescending(x => x.Id),

                StudentField.Skill => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.Skill.KhmerName).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.Skill.KhmerName).ThenByDescending(x => x.Id),

                StudentField.Gender => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.Gender).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.Gender).ThenByDescending(x => x.Id),

                StudentField.SiblingsCount => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.SiblingsCount).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.SiblingsCount).ThenByDescending(x => x.Id),

                StudentField.ExamTable => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.ExamTable).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.ExamTable).ThenByDescending(x => x.Id),

                StudentField.ExamRoom => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.ExamRoom).ThenBy(x => x.Id)
                    : query.OrderByDescending(x => x.ExamRoom).ThenByDescending(x => x.Id),

                _ => options.OrderBy == OrderType.Ascending
                    ? query.OrderBy(x => x.Id)
                    : query.OrderByDescending(x => x.Id),
            };
        }

        private static StudentField ResolveSortField(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return StudentField.Id;
            }

            string key = sortBy.Trim();

            if (Enum.TryParse(key, ignoreCase: true, out StudentField enumField))
            {
                return enumField;
            }

            return key.ToLowerInvariant() switch
            {
                "fullname" or "full_name" => StudentField.FullName,
                "latinfullname" or "latin_full_name" => StudentField.LatinFullName,
                "dob" => StudentField.DateOfBirth,
                "created" => StudentField.CreatedAt,
                _ => StudentField.Id
            };
        }

        private static bool TryParseGender(string? value, out Gender gender)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                gender = default;
                return false;
            }

            string key = value.Trim();

            if (Enum.TryParse(key, ignoreCase: true, out gender))
            {
                return true;
            }

            if (string.Equals(key, "ប្រុស", StringComparison.OrdinalIgnoreCase))
            {
                gender = Gender.Male;
                return true;
            }

            if (string.Equals(key, "ស្រី", StringComparison.OrdinalIgnoreCase))
            {
                gender = Gender.Female;
                return true;
            }

            return false;
        }

        private static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expression)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters);
        }

        private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
            Expression leftBody = new ReplaceParameterVisitor(left.Parameters[0], parameter).Visit(left.Body)!;
            Expression rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body)!;
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody, rightBody), parameter);
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _source;
            private readonly ParameterExpression _target;

            public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
            {
                _source = source;
                _target = target;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _source ? _target : base.VisitParameter(node);
            }
        }
    }
}
