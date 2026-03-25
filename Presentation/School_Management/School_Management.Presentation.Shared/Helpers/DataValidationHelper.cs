using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace School_Management.Presentation.Shared.Helpers
{
    public static class DataValidationHelper
    {
        public static bool HasAllData<T>(this T obj, params string[] ignoreProperties)
        {
            if (obj == null) return false;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (ignoreProperties.Contains(prop.Name)) continue;

                var value = prop.GetValue(obj);

                if (value == null) return false;

                if (value is string str && string.IsNullOrWhiteSpace(str))
                    return false;
            }

            return true;
        }
    }
}
