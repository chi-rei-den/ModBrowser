using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBrowser.Data
{
    public static class PaginatedHelper
    {
        public static IEnumerable<T> ToPaginated<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            else if (pageIndex > (int)Math.Ceiling(count / (double)pageSize))
            {
                pageIndex = (int)Math.Ceiling(count / (double)pageSize);
            }
            return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}