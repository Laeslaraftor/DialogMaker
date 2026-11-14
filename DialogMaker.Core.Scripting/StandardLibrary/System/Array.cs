using System.Collections.Generic;
using Internal.System.Runtime;

namespace System;

public abstract class Array
{
    public abstract int Length { get; }
}
public sealed class Array<T> : Array, IEnumerable<T>
{
    public override int Length => GetLength();
    public unsafe T this[int index]
    {
        get
        {
            if (0 > index || index >= Length)
            {
                throw new IndexOutOfRangeException("Index was outside the bounds of the array.");
            }

            return CompilerServices.GetObjectContentAddress<Array<T>, T>(this)[index];
        }
        set
        {
            if (0 > index || index >= Length)
            {
                throw new IndexOutOfRangeException("Index was outside the bounds of the array.");
            }

            CompilerServices.GetObjectContentAddress<Array<T>, T>(this)[index] = value;
        }
    } 

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private extern int GetLength();

    public static readonly T[] Empty = new T[0];

    public static void Fill(T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
    public static T[] Copy(T[] source)
    {
        T[] destination = new T[source.Length];
        Copy(source, 0, destination, 0, source.Length);

        return destination;
    }
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