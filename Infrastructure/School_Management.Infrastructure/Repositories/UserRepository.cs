using Microsoft.EntityFrameworkCore;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;
using System.Linq.Expressions;

namespace School_Management.Infrastructure.Repositories
{

    public class UserRepository : IUserRepository
    {
        private readonly SchoolDbContext _context;

        public UserRepository(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task AddAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task CreateUserAsync(string username, string plainPassword, string role = "user")
        {
            string hashedPassword = plainPassword.ToHashedPassword();

            User user = new()
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = await _context.Roles.FirstAsync(r => r.Name == role)
            };

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserAsync(string name)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == name);
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, int? page, int pageSize, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users;

            // Apply includes first
            foreach (var include in includes)
                query = query.Include(include);

            // Apply filter
            query = query.Where(predicate);

            // Apply sorting
            if (orderBy != null)
                query = orderBy(query);

            return await query
                .Skip((pageSize * (page - 1)) ?? pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

