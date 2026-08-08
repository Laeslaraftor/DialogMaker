namespace System;

public struct Span<T>
{
    public Span(T[] items)
    {
        _items = items;
    }
    public Span(nint items, int length)
    {
        _itemsPointer = items;
        _length = length;
    }

    public int Length
    {
        get
        {
            if (_items != null)
            {
                return _items.Length;
            }

            return _length;
        }
    }
    public T this[int index]
    {
        get
        {
            if (_items != null)
            {
                return _items[index];
            }
            if (0 > index || index >= _length)
            {
                throw new IndexOutOfRangeException();
            }

            return GetValue(_itemsPointer, index);
        }
        set
        {
            if (_items != null)
            {
                _items[index] = value;
            }
            else
            {
                if (0 > index || index >= _length)
                {
                    throw new IndexOutOfRangeException();
                }

                SetValue(_itemsPointer, index, value);   
            }
        }
    }

    private readonly T[] _items;
    private readonly nint _itemsPointer;
    private readonly int _length;

    public T[] ToArray()
    {
        if (_items != null)
        {
            return _items;
        }

        T[] result = new T[_length];

        for (int i = 0; i < _length; i++)
        {
            result[i] = GetValue(_itemsPointer, i);
        }

        return result;
    }
    public Span<char> Slice(int startIndex, int length)
    {
        return new(_itemsPointer + sizeof(T) * startIndex, length);
    }

    private extern T GetValue(nint items, int index);
    private extern void SetValue(nint items, int index, T value);

    public static implicit operator Span<T>(T[] items) => new Span<T>(items);
}