using System.Collections.Generic;
using System.Collections;
using Internal.System.Runtime;

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
    public static T? FirstOrDefault<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
    {
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                return item;
            }
        }

        return null;
    }
    public static T First<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.FirstOrDefault()
            ?? throw new ArgumentException("No items in sequence");
    }
    public static T First<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
    {
        return enumerable.FirstOrDefault(predicate)
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
    public static T ElementAt<T>(this IEnumerable<T> enumerable, int index)
    {
        return enumerable.ElementAt(index) ??
            throw new ArgumentException("No items on index " + index);
    }
    public static T? ElementAtOrDefault<T>(this IEnumerable<T> enumerable, int index)
    {
        if (0 > index)
        {
            throw new ArgumentException("Index should be greater or equals to zero", nameof(index));
        }
        if (enumerable is T[] array)
        {
            return array[index];
        }
        else if (enumerable is IList<T> list)
        {
            return list[index];
        }
        else if (index == 0)
        {
            return enumerable.FirstOrDefault();
        }

        int currentIndex = 0;

        foreach (var item in enumerable)
        {
            if (currentIndex == index)
            {
                return item;
            }

            currentIndex++;
        }

        return null;
    }
    public static bool Any(this IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            return true;
        }

        return false;
    }
    public static bool Any<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
    {
        foreach (var item in enumerable)
        {
            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }
    public static bool All<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
    {
        foreach (var item in enumerable)
        {
            if (!predicate(item))
            {
                return false;
            }
        }

        return true;
    }
    public static IEnumerable<T> Cast<T>(this IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return (T)item;
        }
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
    public static int Count<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is T[] array)
        {
            return array.Length;
        }
        if (enumerable is ICollection<T> collection)
        {
            return collection.Count;
        }

        int count = 0;

        foreach (var item in enumerable)
        {
            count++;
        }

        return count;
    }
    public static IEnumerable<T> Union<T>(this IEnumerable<T> enumerable, IEnumerable<T> items)
    {
        HashSet<T> values = new();

        foreach (var item in enumerable)
        {
            if (values.Add(item))
            {
                yield return item;
            }
        }
        foreach (var item in items)
        {
            if (values.Add(item))
            {
                yield return item;
            }
        }
    }
    public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, IEnumerable<T> items)
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
    public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> enumerable, int size)
    {
        if (0 >= size)
        {
            throw new ArgumentException("Chunk size should be greater then zero", nameof(size));
        }

        T[] currentChunk = new T[size];
        int currentIndex = 0;

        foreach (var item in enumerable)
        {
            currentChunk[currentIndex] = item;
            currentIndex++;

            if (currentIndex >= size)
            {
                yield return currentChunk;
                currentIndex = 0;
                currentChunk = new T[size];
            }
        }

        yield return currentChunk;
    }
    public static IEnumerable<T> Skip<T>(this IEnumerable<T> enumerable, int count)
    {
        int skipped = 0;

        foreach (var item in enumerable)
        {
            if (skipped >= count)
            {
                yield return item;
            }

            skipped++;
        }
    }
    public static IEnumerable<T> Take<T>(this IEnumerable<T> enumerable, int count)
    {
        int taked = 0;

        foreach (var item in enumerable)
        {
            if (taked >= count)
            {
                yield break;
            }

            yield return item;
            taked++;
        }
    }

    public static T[] ToArray<T>(this IEnumerable<T> enumerable)
    {
        T[] result;

        if (enumerable is T[] array)
        {
            int length = array.Length;

            if (length == 0)
            {
                return Array<T>.Empty;
            }

            result = new T[length];
            Array<T>.Copy(array, 0, result, 0, length);
        }
        else if (enumerable is ICollection<T> collection)
        {
            int length = collection.Count;

            if (length == 0)
            {
                return Array<T>.Empty;
            }

            result = new T[length];
            
            if (enumerable is IList<T> list)
            {
                list.CopyTo(0, result, 0, length);
            }
            else
            {
                int i = 0;

                foreach (var item in enumerable)
                {
                    result[i] = item;
                    i++;
                }
            }
        }
        else
        {
            var enumerator = enumerable.GetEnumerator();
            int length = 0;

            foreach (var item in enumerator)
            {
                length++;
            }

            if (length == 0)
            {
                return Array<T>.Empty;
            }
            
            result = new T[length];
            int i = 0;

            foreach (var item in enumerator)
            {
                result[i] = item;
                i++;
            }
        }

        return result;
    }
    public static List<T> ToList<T>(this IEnumerable<T> enumerable)
    {
        return new(enumerable.ToArray(), false);
    }
}