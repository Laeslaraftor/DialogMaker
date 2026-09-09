using System.Collections.Generic;
using System.Collections;

namespace System.Linq;

public static class Enumerator
{
    public static T? FirstOrDefault<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            return item;
        }

        return null;
    }
    public static T First<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            return item;
        }

        throw new ArgumentException("No items in sequence");
    }
    public static T? LastOrDefault<T>(this IEnumerable<T> enumerable)
    {
        T? lastItem = null;

        foreach (var item in enumerable)
        {
            lastItem = item;
        }

        return lastItem;
    }
    public static T Last<T>(this IEnumerable<T> enumerable)
    {
        T? lastItem = null;

        foreach (var item in enumerable)
        {
            lastItem = item;
        }

        return lastItem ??
            throw new ArgumentException("No items in sequence");
    }
    public static bool Any(this IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            return true;
        }

        return false;
    }
    public static bool Contains<T>(this IEnumerable<T> enumerable, T item)
    {
        foreach (var enumerableItem in enumerable)
        {
            if (Equals(enumerableItem, item))
            {
                return true;
            }
        }

        return false;
    }
    public static int Count(this IEnumerable enumerable)
    {
        int count = 0;

        foreach (var item in enumerable)
        {
            count++;
        }

        return count;
    }
    public static IEnumerable<T> Union<T>(this IEnumerable<T> enumerable, IEnumerable<T> items)
    {
        return new UnionEnumerable<T>(enumerable, items);
    }

    private class UnionEnumerable<T> : IEnumerable<T>
    {
        public UnionEnumerable(IEnumerable<T> first, IEnumerable<T> second)
        {
            _first = first;
            _second = second;
        }

        private readonly IEnumerable<T> _first;
        private readonly IEnumerable<T> _second;

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(_first.GetEnumerator(), _second.GetEnumerator());
        }

        private class Enumerator : IEnumerator<T>
        {
            public Enumerator(IEnumerator<T> first, IEnumerator<T> second)
            {
                _first = first;
                _second = second;
            }

            public T Current { get; private set; }

            private readonly IEnumerator<T> _first;
            private readonly IEnumerator<T> _second;
            private bool _nowSecond;
            private bool _completed;

            public bool MoveNext()
            {
                if (_completed)
                {
                    return false;
                }

                IEnumerator<T> enumerator = _nowSecond ? _second : _first;

                if (enumerator.MoveNext())
                {
                    Current = enumerator.Current;
                    return true;
                }            
                if (_nowSecond)
                {
                    _completed = true;
                    return false;
                }

                _nowSecond = true;

                if (_second.MoveNext())
                {
                    Current = _second.Current;
                    return true;
                }

                _completed = true;

                return false;
            }

            public void Reset()
            {
                _first.Reset();
                _second.Reset();
                _nowSecond = false;
                _completed = false;
            }
        }
    }
}