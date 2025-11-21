using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DialogMaker
{
    public partial class App : Application
    {
        public static bool TryFindResource<T>(string name, [NotNullWhen(true)] out T? result)
        {
            result = default;

            if (Current == null)
            {
                return false;
            }

            var resource = Current.TryFindResource(name);

            if (resource is T typedResource)
            {
                result = typedResource;
            }

            return result != null;
        }
    }

}
