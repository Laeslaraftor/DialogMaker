namespace System.Collections.Generic;

public interface IList<T> : IEnumerable<T>
{
    public int Count { get; }
    public T this[int index] { get; set; }

    public int IndexOf(T item);
    public bool Contains(T item);
    public void Add(T item);
    public void Insert(T item, int index);
    public bool Remove(T item);
    public void Clear();
}