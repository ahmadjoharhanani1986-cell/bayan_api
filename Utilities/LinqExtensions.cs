using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SHLAPI.Utilities
{
    public static class LinqExtensions
    {
        public static bool In<T>(this T source, params T[] list)
        {
            return list.Contains(source);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this ICollection source, Func<TSource, TResult> selector)
        {
            var list = new List<TResult>(source.Count);
            foreach (TSource item in source)
                list.Add(selector(item));
            return list;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}