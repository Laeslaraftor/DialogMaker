namespace DialogMaker.Core.Editor
{
    public readonly struct PreferenceChangedEventArgs(string key, object? oldValue, object? newValue)
    {
        public string Key { get; } = key;
        public object? OldValue { get; } = oldValue;
        public object? NewValue { get; } = newValue;
    }
}
