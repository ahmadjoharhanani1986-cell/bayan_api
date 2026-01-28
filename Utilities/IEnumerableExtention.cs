using System.Collections.Generic;

namespace SHLAPI
{
    public static class IEnumerableExtention
    {
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return null;
            }
            return new List<TSource>(source);
        }
    }
}