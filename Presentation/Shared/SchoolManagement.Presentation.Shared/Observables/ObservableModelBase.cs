using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

public class ObservableModelBase<T> : ObservableObject where T : class, new()
{
    public T Model { get; }

    public ObservableModelBase(T model)
    {
        Model = model;
    }

    // A dynamic indexer that your UI can bind to directly
    public object? this[string propertyName]
    {
        get
        {
            PropertyInfo? prop = typeof(T).GetProperty(propertyName);
            return prop?.GetValue(Model);
        }
        set
        {
            PropertyInfo? prop = typeof(T).GetProperty(propertyName);
            if (prop != null && prop.CanWrite)
            {
                object? currentValue = prop.GetValue(Model);
                if (!Equals(currentValue, value))
                {
                    // EF/Backend gets the value immediately in memory
                    prop.SetValue(Model, value);

                    // UI gets notified immediately without manual string typing
                    OnPropertyChanged($"Item[{propertyName}]");
                }
            }
        }
    }
}