using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<SchoolDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=SchoolDB;Username=postgres;Password=Internet@2008")
            .Options;

        using var context = new SchoolDbContext(options);

        Permission permission = new()
        {
            Name = "All",
        };

        Role role = new()
        {
            Name = "Admin",
            Description = "A role that controls everything",
            Permissions = [permission]
        };

        var user = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            Role = role
        };

        context.Permissions.Add(permission);
        context.Roles.Add(role);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        Console.WriteLine("User inserted successfully!");
    }
}