using System;
using System.Collections.Generic;

namespace Raft.Infrastructure.Extensions
{
    internal static class EnumerableUtilities
    {
        public static IEnumerable<long> Range(long start, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            for (var i = 0; i < count; ++i)
                yield return start + i;
        }
    }
}
