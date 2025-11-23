namespace DialogMaker.Lib
{
    public class EditCommandEventArgs<T>(T oldValue, T newValue, object? parameter = null) 
        : ValueChangedEventArgs<T>(oldValue, newValue)
    {
        public object? Parameter { get; } = parameter;
    }
}
