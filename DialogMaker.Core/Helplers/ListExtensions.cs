using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace DialogMaker.Core
{
    public static class ListExtensions
    {
        public static async Task<T> AsTask<T>(this Func<IEnumerable<ProgressResult<T>>> func)
        {
            return await Task.Run(() =>
            {
                T? result = default;

                foreach (var item in func())
                {
                    result = item.Value;
                }

                if (result == null)
                {
                    throw new ArgumentException("Пустой результат");
                }

                return result;
            });
        }

        public static ReadOnlyDictionary<TKey, ReadOnlyCollection<TValue>> ToReadonly<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictionary) 
            where TKey : notnull
        {
            Dictionary<TKey, ReadOnlyCollection<TValue>> result = [];

            foreach (var info in dictionary)
            {
                result.Add(info.Key, new(info.Value));
            }

            return new(result);
        }

        public static void Invert<T>(this IList<T> list)
        {
            List<T> copy = [.. list];
            int count = list.Count - 1;

            for (int i = 0; i < count + 1; i++)
            {
                copy[i] = list[count - i];
            }
        }
        public static bool TryGetValue<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, [NotNullWhen(true)] out T? result)
        {
            result = default;

            foreach (var item in enumerable)
            {
                if (predicate(item))
                {
                    result = item;
#pragma warning disable CS8762
                    return true;
#pragma warning restore CS8762
                }
            }

            return false;
        }
        //public static List<TSelection> Select<TSelection, TItem>(this IEnumerable<TItem> items, Func<TItem, TSelection> selector)
        //{
        //    List<TSelection> result = new();

        //    foreach (var item in items)
        //    {
        //        result.Add(selector(item)); 
        //    }

        //    return result;
        //}
    }
}
