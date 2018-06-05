namespace OJS.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize) =>
            source
                .Select((x, i) => new
                {
                    Index = i,
                    Value = x
                })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value));
    }
}