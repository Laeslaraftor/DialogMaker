namespace System.Collections.Generic;

public interface IEnumerator<T> : IEnumerator
{
    public T Current { get; }
}