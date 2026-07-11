namespace System;

using System.Collections.Generic;

public sealed class Array<T> : IEnumerable<T>
{
    public int Length => GetLength();
    public T this[int index]
    {
        get => GetItem(index);
        set => SetItem(index, value);
    } 

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private extern int GetLength();
    private extern T GetItem(int index);
    private extern void SetItem(int index, T item);

    public static readonly T[] Empty = new T[0];

    public static void Copy(T[] source, T[] destination)
    {
        Copy(source, 0, destination, 0, Math.Min(source.Length, destination.Length));
    }
    public static void Copy(T[] source, T[] destination, int count)
    {
        Copy(source, 0, destination, 0, count);
    }
    public static void Copy(T[] source, int sourceIndex, T[] destination, int destinationIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            destination[i + destinationIndex] = source[sourceIndex + i];
        }
    }

    private class Enumerator : IEnumerator<T>
    {
        public Enumerator(T[] items)
        {
            _items = items;
        }

        public T Current { get; private set; }

        private readonly T[] _items;
        private int _currentIndex = -1;
        
        public bool MoveNext()
        {
            if (_items.Length == 0 ||
                _currentIndex + 1 >= _items.Length)
            {
                return false;
            }
            
            _currentIndex++;
            Current = _items[_currentIndex];

            return true;
        }
        public void Reset()
        {
            Current = null;
            _currentIndex = -1;
        }
    }
}