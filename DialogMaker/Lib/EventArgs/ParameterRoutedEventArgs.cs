using System.Windows;

namespace DialogMaker.Lib
{
    public readonly struct ParameterRoutedEventArgs(RoutedEventArgs e, object? parameter)
    {
        public RoutedEventArgs RoutedEventArgs { get; } = e;
        public object? Parameter { get; } = parameter;
    }
}
