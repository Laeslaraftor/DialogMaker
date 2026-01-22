using DialogMaker.Core.Executioning;
using System.Windows;

namespace DialogMaker.Lib
{
    public readonly struct OperationArgumentClickEventArgs(Operation operation, int argumentIndex, RoutedEventArgs routedEventArgs)
    {
        public Operation Operation { get; } = operation;
        public int ArgumentIndex { get; } = argumentIndex;
        public int ArgumentValue => Operation.Arguments[ArgumentIndex];
        public RoutedEventArgs RoutedEventArgs { get; } = routedEventArgs;
    }
}
