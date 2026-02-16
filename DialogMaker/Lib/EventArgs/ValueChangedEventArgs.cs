using System.Windows;

namespace DialogMaker.Lib
{
    public class ValueChangedEventArgs<T>(T oldValue, T newValue) : EventArgs
    {
        public ValueChangedEventArgs(DependencyPropertyChangedEventArgs args)
            : this((T)args.OldValue, (T)args.NewValue)
        {
        }

        public T OldValue { get; } = oldValue;
        public T NewValue { get; } = newValue;

        #region Операторы

        public static implicit operator ValueChangedEventArgs<T>(DependencyPropertyChangedEventArgs args)
        {
            return new(args);
        }

        #endregion
    }
}
