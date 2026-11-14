namespace System;

public unsafe struct Span<T>
{
    public Span(T[] items)
    {
        _items = items;
    }
    public Span(T* items, int length)
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

            return _itemsPointer[index];
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

                _itemsPointer[index] = value;
            }
        }
    }

    private readonly T[] _items;
    private readonly T* _itemsPointer;
    private readonly int _length;

    public T[] ToArray()
    {
        if (_items != null)
        {
            return Array<T>.Copy(_items);
        }

        T[] result = new T[_length];

        for (int i = 0; i < _length; i++)
        {
            result[i] = _itemsPointer[i];
        }

        return result;
    }
    public Span<T> Slice(int startIndex, int length)
    {
        return new(_itemsPointer + sizeof(T) * startIndex, length);
    }

    public static implicit operator Span<T>(T[] items) => new(items);
}