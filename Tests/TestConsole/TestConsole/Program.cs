using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using School_Management.Core.Enums;
using School_Management.Core.Models;
using School_Management.Infrastructure.Data;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Program program = new();

        var options = new DbContextOptionsBuilder<SchoolDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=SchoolDB;Username=postgres;Password=Internet@2008")
            .Options;

        using SchoolDbContext context = new(options);

        //Permission[] permissions = await context.Permissions.ToArrayAsync();
        //Role headTeacher = await context.Roles.FirstAsync(r => r.Name == "HeadTeacher");

        //headTeacher.Permissions = permissions;
        //context.Update(headTeacher);

        await context.SaveChangesAsync();
        Console.WriteLine("Operation is successful");
        Console.ReadKey();
    }

    public Class[] GenerateComputerClasses()
    {
        Class year1 = new()
        {
            GenerationId = 71,
            GradeId = 1,
        };
        Class year2 = new()
        {
            GenerationId = 68,
            GradeId = 2
        };
        Class year3 = new()
        {
            GenerationId = 67,
            GradeId = 3,
        };

        return [year1, year2, year3];
    }

    public Class[] GenerateElectricalClasses()
    {
        Class year1 = new()
        {
            GenerationId = 72,
            GradeId = 1,
        };
        Class year2 = new()
        {
            GenerationId = 69,
            GradeId = 2
        };
        Class year3 = new()
        {
            GenerationId = 66,
            GradeId = 3,
        };

        return [year1, year2, year3];
    }

    public Class[] GenerateCNCClasses()
    {
        Class year1 = new()
        {
            GenerationId = 73,
            GradeId = 1,
        };
        Class year2 = new()
        {
            GenerationId = 70,
            GradeId = 2
        };

        return [year1, year2];
    }
}