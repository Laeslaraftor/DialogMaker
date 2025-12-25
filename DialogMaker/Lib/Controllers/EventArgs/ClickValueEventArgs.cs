using System.Windows;

namespace DialogMaker.Lib.Controllers
{
    public readonly struct ClickValueEventArgs<T>(T value, RoutedEventArgs eventArgs)
    {
        public RoutedEventArgs ClickEventArgs { get; } = eventArgs;
        public T Value { get; } = value;
    }
}
