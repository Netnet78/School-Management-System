using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SchoolManagement.Core.Shared.Configurations;

namespace SchoolManagement.Infrastructure.Data
{
    public class SchoolDbContextFactory : IDesignTimeDbContextFactory<SchoolDbContext>
    {
        public SchoolDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SchoolDbContext>();

            string connectionString = SecretHelper.GetValueFromEnv("DB_CONNECTION");

            // Use Npgsql with connection pooling for better performance
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
            return new SchoolDbContext(optionsBuilder.Options);
        }
    }
}
