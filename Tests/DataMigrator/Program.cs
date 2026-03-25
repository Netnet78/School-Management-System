using Npgsql;
using School_Management.Application;
using School_Management.Application.Services;
using School_Management.Core.Enums;
using School_Management.Core.Interfaces;
using School_Management.Core.Models;
using School_Management.Infrastructure.Services;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

internal class Program
{
    private static readonly IS3Service s3Service = new S3Service();
    private static readonly ISettingsService settingsService = new SettingsService();
    private static readonly IPhotoUploadService uploadService = new PhotoUploadService(settingsService, s3Service);

    private static void Main(string[] args)
    {

        string accessFile = @"D:\Office Projects\MS Access Projects\Technical Student Database\StundentandstaffData(update2026).accdb";
        string connString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={accessFile};Persist Security Info=False;";
        string pgConn = "Host=localhost;Port=5432;Username=postgres;Password=Internet@2008;Database=SchoolDB";

        //DataTable classes = GetClassesFromAccess(connString);
        //BulkInsertClasses(pgConn, classes);

        //DataTable students = GetStudentsFromAccess(connString);
        //BulkInsertStudents(pgConn, students);

        //Console.WriteLine("Migration finished.");

        InsertStudent(pgConn);

        Console.WriteLine("Success!!");

        Console.ReadKey();
    }

    static void InsertStudent(string pgConnectionString)
    {
        using var conn = new NpgsqlConnection(pgConnectionString);
        conn.Open();

        // 1. Read all candidate IDs first
        List<int> candidateIds = new();

        string selectSql = @"SELECT ""Id"" FROM ""Candidates""";

        using (var cmd = new NpgsqlCommand(selectSql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                candidateIds.Add(reader.GetInt32(0));
            }
        } // reader is closed here

        Console.WriteLine($"Found {candidateIds.Count} candidates.");

        // 2. Now insert each candidate into Students
        string insertSql = @"
        INSERT INTO public.""Students"" 
        (""Id"", ""EnrollDate"", ""CreatedAt"", ""CandidateId"")
        VALUES (@id, @enrollDate, @createdAt, @candidateId)
    ";

        foreach (var candidateId in candidateIds)
        {
            using var insertCmd = new NpgsqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("id", candidateId);
            insertCmd.Parameters.AddWithValue("enrollDate", DBNull.Value); // null
            insertCmd.Parameters.AddWithValue("createdAt", DateTime.Now);
            insertCmd.Parameters.AddWithValue("candidateId", candidateId);

            insertCmd.ExecuteNonQuery();
        }

        Console.WriteLine("All candidates inserted into Students!");
    }
    static DataTable GetStudentsFromAccess(string connString)
    {
        using var conn = new OleDbConnection(connString);

        string query = @"SELECT s.*, c.ClassName
                                    FROM tblStudents AS s
                                    INNER JOIN tblClass AS c
                                    ON s.ClassID = c.ClassID;";

        var adapter = new OleDbDataAdapter(query, conn);

        DataTable table = new DataTable();
        adapter.Fill(table);

        Console.WriteLine($"Loaded {table.Rows.Count} students from Access");

        return table;
    }
    static DataTable GetClassesFromAccess(string connString)
    {
        using OleDbConnection conn = new(connString);

        string query = @"SELECT ClassID, ClassName, Year FROM tblClass ";
        OleDbDataAdapter adapter = new(query, conn);

        DataTable table = new();
        adapter.Fill(table);

        Console.WriteLine($"Loaded {table.Rows.Count} classes from Access");

        return table;
    }

    static async Task BulkInsertStudents(string pgConnectionString, DataTable students)
    {
        using var conn = new NpgsqlConnection(pgConnectionString);
        conn.Open();

        using var writer = conn.BeginBinaryImport(@"
            COPY ""Candidates"" (
                ""Id"",
                ""LastName"",
                ""FirstName"",
                ""LatinLastName"",
                ""LatinFirstName"",
                ""Gender"",
                ""DateOfBirth"",
                ""SkillId"",
                ""BirthProvince"",
                ""BirthVillage"",
                ""BirthCommune"",
                ""BirthDistrict"",
                ""PhoneNumber"",
                ""ExamCenter"",
                ""ExamDate"",
                ""ExamTable"",
                ""ExamRoom"",
                ""FromSchool"",
                ""OtherInfo"",
                ""FatherName"",
                ""FatherOccupation"",
                ""MotherName"",
                ""MotherOccupation"",
                ""SiblingsCount"",
                ""Religion"",
                ""StayType"",
                ""PhotoKey"",
                ""CreatedAt""
            ) FROM STDIN (FORMAT BINARY)");

        int index = 0;

        foreach (DataRow row in students.Rows)
        {
            writer.StartRow();

            int id = (int)row["StudID"];
            string[] name = ((string)row["StudName"]).Split(" ");
            string[] latinName = ((string)row["LatangName"]).Split(" ");

            writer.Write(id);
            writer.Write(name[0]);
            writer.Write(string.Join(" ", name.Skip(1)));
            writer.Write(latinName[0]);
            writer.Write(string.Join(" ", latinName.Skip(1)));
            writer.Write((string)row["Sex"] == "Male" ? Gender.Male.ToString() : Gender.Female.ToString());
            if (row["DateOfBirth"] is DBNull)
                writer.Write(new DateOnly(1000, 1, 1));
            else
                writer.Write(DateOnly.FromDateTime((DateTime)row["DateOfBirth"]));

            string department = ((string)row["ClassName"]).Split(" ")[0];
            int skillId = 0;

            switch (department)
            {
                case "Computer":
                    skillId = 1;
                    break;
                case "Electrical":
                    skillId = 2;
                    break;
                case "CNC":
                    skillId = 3;
                    break;
                case "Automotive":
                    skillId = 4;
                    break;
                case "Sewing":
                    skillId = 5;
                    break;
                case "Academic":
                    skillId = 6;
                    break;
            }

            writer.Write(skillId);
            writer.Write(row["PlaceOfBirth"] is DBNull ? "" : row["PlaceOfBirth"]);
            writer.Write(string.Empty);
            writer.Write(string.Empty);
            writer.Write(string.Empty);
            writer.Write(string.Empty);
            writer.Write(string.Empty);
            writer.Write(new DateOnly(1000,1,1));
            writer.Write(0);
            writer.Write(0);
            writer.Write(string.Empty);
            writer.Write(string.Empty);
            writer.Write(row["FatherName"] is DBNull ? "" : row["FatherName"]);
            writer.Write(row["FatherCareers"] is DBNull ? "" : row["FatherCareers"]);
            writer.Write(row["MotherName"] is DBNull ? "" : row["MotherName"]);
            writer.Write(row["MotherCareers"] is DBNull ? "" : row["MotherCareers"]);
            writer.Write(row["Sibling"] is DBNull ? 0 : row["Sibling"]);
            writer.Write(row["Religions"] is DBNull ? "" : row["Religions"]);
            writer.Write((bool)row["Stay"] ? StudentStayType.Inside.ToString() : StudentStayType.Outside.ToString());

            string photoPath = Path.Combine("E:/AccessPhotos/All", $"{id}_{row["LatangName"]}.jpg");

            if (Path.Exists(photoPath))
            {
                FileObject file = await uploadService.UploadStudentPhoto(photoPath);
                writer.Write(file.FileName);
            }
            else
            {
                writer.Write(string.Empty);
            }

            writer.Write(DateTime.Now);
        }

        writer.Complete();

        Console.WriteLine("Bulk insert completed.");
    }
    static void BulkInsertClasses(string pgConn, DataTable classes)
    {
        using var conn = new NpgsqlConnection(pgConn);
        conn.Open();

        using var writer = conn.BeginBinaryImport(@"
            COPY ""Generations"" (
                ""Id"",
                ""CohortNumber"",
                ""AcademicStartYear"",
                ""AcademicEndYear"",
                ""DepartmentId""
            ) FROM STDIN (FORMAT BINARY)");

        foreach (DataRow row in classes.Rows)
        {
            writer.StartRow();

            int id = (int)row["ClassID"];
            int cohortNumber = 0;

            Match match = Regex.Match((string)row["ClassName"], @"\d+");

            if (match.Success)
            {
                cohortNumber = int.Parse(match.Value);
            }

            string[] years = ((string)row["Year"]).Split("-");
            int start = int.Parse(years[0]);
            int end = int.Parse(years[1]);

            string department = ((string)row["ClassName"]).Split(" ")[0];
            int depId = 0;

            switch (department)
            {
                case "Computer":
                    depId = 1;
                    break;
                case "Electrical":
                    depId = 2;
                    break;
                case "CNC":
                    depId = 3;
                    break;
                case "Automotive":
                    depId = 4;
                    break;
                case "Sewing":
                    depId = 5;
                    break;
                case "Academic":
                    depId = 6;
                    break;
            }

            writer.Write(id);
            writer.Write(cohortNumber);
            writer.Write(start);
            writer.Write(end);
            writer.Write(depId);
        }

        writer.Complete();

        Console.WriteLine("Bulk insert completed.");
        Console.ReadKey();
    }
}