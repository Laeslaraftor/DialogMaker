namespace DialogMaker.Lib
{
    public readonly struct EditCommandEventArgs<T>(T oldValue, T newValue, object? parameter = null)
    {
        public T OldValue { get; } = oldValue;
        public T NewValue { get; } = newValue;
        public object? Parameter { get; } = parameter;

        #region Управление

        public static implicit operator ValueChangedEventArgs<T>(EditCommandEventArgs<T> e)
        {
            return new(e.OldValue, e.NewValue);
        }

        #endregion
    }
}
