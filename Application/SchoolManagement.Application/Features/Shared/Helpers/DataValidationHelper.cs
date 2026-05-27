using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SchoolManagement.Application.Features.Shared.Helpers
{
    public static class DataValidationHelper
    {
        public static ValidationResponse HasAllData<T>(this T obj, params string[] ignoreProperties)
        {
            if (obj == null) return new() { IsValid = false };

            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<ValidationError> missing = [];

            foreach (var prop in properties)
            {
                if (ignoreProperties.Contains(prop.Name)) continue;

                var value = prop.GetValue(obj);

                if (value == null || value is string str && string.IsNullOrWhiteSpace(str))
                    missing.Add(new() { PropertyName = prop.Name, ErrorMessage = $"{prop.Name} cannot be empty." });
            }

            if (missing.Count() >= 1)
            {
                return new() { IsValid = false, MissingProperties = missing.ToArray() };
            }

            return new() { IsValid = true };
        }

        public static ValidationResponse HasAllRequiredData<T>(this T obj)
        {
            if (obj == null) return new() { IsValid = false };

            ValidationContext context = new(obj);
            List<ValidationResult> results = [];

            bool isValid = Validator.TryValidateObject(obj, context, results, true);

            ValidationError[] errors = results.SelectMany(r => r.MemberNames.Select(m => new ValidationError
            {
                PropertyName = m,
                ErrorMessage = r.ErrorMessage ?? string.Empty,
            })).ToArray();

            return new() { IsValid = isValid, MissingProperties = errors };
        }
    }
}
