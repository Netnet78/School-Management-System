using School_Management.Core.Models;
using System.Reflection;

namespace School_Management.Application.Services
{
    public static class CandidateValidationService
    {
        public static bool HasAllData(this Candidate s)
        {
            var properties = typeof(Candidate).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(s);

                // Skip some properties
                if (prop.Name == nameof(Candidate.Id)
                    || prop.Name == nameof(Candidate.Age)
                    || prop.Name == nameof(Candidate.OtherInfo)
                    || prop.Name == nameof(Candidate.CreatedAt)
                    || prop.Name == nameof(Candidate.LatinFullName)
                    || prop.Name == nameof(Candidate.FullName))
                    continue;

                if (value == null) return false;

                if (value is string str && string.IsNullOrWhiteSpace(str))
                    return false;
            }

            return true;
        }
    }
}
