using Microsoft.Extensions.DependencyInjection;

namespace School_Management.Core.Helpers
{
    public static class PropertiesHelper
    {
        public static Type Clone<Type>(this Type source)
        {
            Type result = Activator.CreateInstance<Type>();
            var props = typeof(Type).GetProperties();
            foreach (var prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(result, prop.GetValue(source, null), null);
                }
            }
            return result;
        }
    }
}
