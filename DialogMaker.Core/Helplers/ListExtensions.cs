using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core
{
    public static class ListExtensions
    {
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
