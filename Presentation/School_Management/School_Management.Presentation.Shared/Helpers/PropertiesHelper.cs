namespace School_Management.Presentation.Shared.Helpers
{
    public static class PropertiesHelper
    {
        public static Type Clone<Type>(this Type source)
        {
            Type result = default!;
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
