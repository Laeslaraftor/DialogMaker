namespace System.Collections.Generic;

public interface ICollection<T> : IEnumerable<T>
{
    public int Count { get; }

    public void Add(T item);
    public bool Remove(T item);
    public void Clear();
}