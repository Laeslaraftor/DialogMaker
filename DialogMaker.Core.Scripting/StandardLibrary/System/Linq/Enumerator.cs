using System.Collections.Generic;
using System.Collections;

namespace System.Linq;

public static class Enumerator
{
    public static T? FirstOrDefault<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is T[] array)
        {
            if (array.Length == 0)
            {
                return null;
            }

            return array[0];
        }
        if (enumerable is IList<T> list)
        {
            if (list.Count == 0)
            {
                return null;
            }

            return list[0];
        }

        foreach (var item in enumerable)
        {
            return item;
        }

        return null;
    }
    public static T First<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.LastOrDefault()
            ?? throw new ArgumentException("No items in sequence");
    }
    public static T? LastOrDefault<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is T[] array)
        {
            int length = array.Length;

            if (length == 0)
            {
                return null;
            }

            return array[length - 1];
        }
        else if (enumerable is IList<T> list)
        {
            int count = list.Count;

            if (count == 0)
            {
                return null;
            }

            return list[count];
        }

        T? lastItem = null;

        foreach (var item in enumerable)
        {
            lastItem = item;
        }

        return lastItem;
    }
    public static T Last<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.LastOrDefault() ??
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
        foreach (var item in enumerable)
        {
            yield return item;
        }
        foreach (var item in items)
        {
            yield return item;
        }
    }
    public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
    {
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }
    public static IEnumerable<TValue> Select<TItem, TValue>(this IEnumerable<TItem> enumerable, Func<TItem, TValue> selector)
    {
        foreach (var item in enumerable)
        {
            yield return selector(item);
        }
    }
    public static IEnumerable<TValue> SelectMany<TItem, TValue>(this IEnumerable<TItem> enumerable, Func<TItem, IEnumerable<TValue>> selector)
    {
        foreach (var item in enumerable)
        {
            foreach (var innerItem in selector(item))
            {
                yield return innerItem;
            }
        }
    }
    public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T value)
    {
        foreach (var item in enumerable)
        {
            yield return item;
        }

        yield return value;
    }
    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T value)
    {
        yield return value;

        foreach (var item in enumerable)
        {
            yield return item;
        }
    }
}