namespace SchoolManagement.Core.Shared.Extensions
{
    public static class PropertiesHelper
    {
        public static T Clone<T>(this T source) where T : new()
        {
            if (source == null) throw new ArgumentNullException("The source object cannot be null!");

            T clone = FastCloner.FastCloner.DeepClone(source)!;
            return clone;
        }
    }
}
