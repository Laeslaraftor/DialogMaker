namespace System.Native;

public struct NativeArray<T> where T : struct
{
    public NativeArray(Pointer<T> items, int length)
    {
        _items = items;
        _length = length;
    }

    public int Length => _length;
    public T this[int index]
    {
        get
        {
            if (0 > index || index >= _length)
            {
                throw new IndexOutOfRangeException();
            }

            return _items[index];
        }
        set
        {
            if (0 > index || index >= _length)
            {
                throw new IndexOutOfRangeException();
            }

            _items[index] = value;
        }
    }

    private readonly Pointer<T> _items;
    private readonly int _length;

    public Span<T> ToSpan() => new Span<T>((nint)_items, _length);
    public T[] ToArray() => ToSpan().ToArray();
}