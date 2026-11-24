using System.ComponentModel;

namespace DialogMaker.Core.Scripting
{
    internal static class ObservableObject
    {
        private static Dictionary<string, PropertyChangedEventArgs> _propertyChangedEventArgs = [];

        public static PropertyChangedEventArgs GetPropertyChangedEventArgs(string propertyName)
        {
            lock (_propertyChangedEventArgs)
            {
                if (!_propertyChangedEventArgs.TryGetValue(propertyName, out var args))
                {
                    args = new(propertyName);
                    _propertyChangedEventArgs.Add(propertyName, args);
                }

                return args;
            }
        }
    }
}
