using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using SchoolManagement.Core.Shared.Attributes;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolManagement.Infrastructure.Shared.Data
{
    public static class JsonDataHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters =
                {
                    new JsonStringEnumConverter()
                }
        };

        /// <summary>
        /// Serializes the property values to a JSON string, excluding or masking properties marked with the
        /// <see cref="AuditHideAttribute"/>.
        /// </summary>
        /// <remarks>Properties decorated with the <see cref="AuditHideAttribute"/> are either omitted or their values
        /// are replaced with a masked value in the resulting JSON. This method is intended for scenarios where
        /// sensitive data should not be exposed in serialized output.</remarks>
        /// <param name="values">The set of property values to serialize. Cannot be null.</param>
        /// <returns>A JSON string representing the serialized property values. Properties marked with the <see cref="AuditHideAttribute"/> are
        /// excluded or masked in the output.</returns>
        public static string SerializeProperties(this PropertyValues values)
        {
            Dictionary<string, object?> dict = new();

            foreach (IProperty property in values.Properties)
            {
                PropertyInfo? propertyInfo = property.PropertyInfo;

                if (propertyInfo == null)
                    continue;

                if (propertyInfo.IsDefined(typeof(AuditHideAttribute)))
                    continue;

                object? value = values[property.Name];

                AuditHideAttribute? attribute = property
                    .PropertyInfo?
                    .GetCustomAttributes(typeof(AuditHideAttribute), true)
                    .Cast<AuditHideAttribute>()
                    .FirstOrDefault();

                if (attribute != null && attribute.ShowMasked == true)
                {
                    value = "***MASKED***";
                }
                else if (attribute != null && attribute.ShowMasked == false)
                {
                    continue;
                }

                dict[property.Name] = value;
            }

            return JsonSerializer.Serialize(dict, _jsonOptions);
        }

        /// <summary>
        /// Serializes the properties of the current object to a JSON string, including only those properties that
        /// differ from the comparison object if specified.
        /// </summary>
        /// <remarks>Properties decorated with the <see cref="AuditHideAttribute"/> are masked in the serialized output
        /// for security. The comparison is performed using value equality for each property.</remarks>
        /// <param name="current">The set of property values to serialize.</param>
        /// <param name="comparison">The set of property values to compare against for detecting modifications.</param>
        /// <param name="onlyModified">If <see langword="true"/>, only properties with values different from the comparison object are included;
        /// otherwise, all properties are included.</param>
        /// <returns>A JSON string representing the selected properties and their values. Properties marked with the
        /// <see cref="AuditHideAttribute"/> are masked in the output.</returns>
        public static string SerializeModifiedProperties(
            this PropertyValues current,
            PropertyValues comparison,
            bool onlyModified)
        {
            Dictionary<string, object?> dict = [];

            foreach (IProperty property in current.Properties)
            {
                object? currentValue = current[property.Name];
                object? comparisonValue = comparison[property.Name];

                bool changed = !Equals(currentValue, comparisonValue);

                if (!onlyModified || changed)
                {
                    AuditHideAttribute? attribute = property
                        .PropertyInfo?
                        .GetCustomAttributes(typeof(AuditHideAttribute), true)
                        .Cast<AuditHideAttribute>()
                        .FirstOrDefault();

                    if (attribute != null && attribute.ShowMasked == true)
                    {
                        dict[property.Name] = "***MASKED***";
                        continue;
                    }
                    else if (attribute != null && attribute.ShowMasked == false)
                    {
                        continue;
                    }

                    dict[property.Name] = currentValue;
                }
            }

            return JsonSerializer.Serialize(dict, _jsonOptions);
        }
    }
}
