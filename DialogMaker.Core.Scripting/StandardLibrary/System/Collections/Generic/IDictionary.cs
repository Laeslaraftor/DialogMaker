namespace System.Collections.Generic;

public interface IDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    public int Count { get; }
    public TValue this[TKey key] { get; set; }

    public bool ContainsKey(TKey key);
    public void Add(TKey key, TValue value);
    public bool TryAdd(TKey key, TValue value);
    public bool Remove(TKey key);
    public void Clear();
}