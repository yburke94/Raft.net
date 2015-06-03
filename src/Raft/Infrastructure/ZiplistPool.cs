using System.Collections.Concurrent;

namespace Raft.Infrastructure
{
    /// <summary>
    /// Manages a pool of Ziplists that have their underlying buffer on the Large Object Heap.
    /// The pool allows you to re-use these Ziplists in order to reduce LOH fragmentation.
    /// </summary>
    internal class ZiplistPool
    {
        private const int LohTargetSize = 85000;

        // TODO: What happens if pool gets too large? De-allocate Ziplists? Force LOH compaction?
        private readonly ConcurrentBag<Ziplist> _ziplists = new ConcurrentBag<Ziplist>();

        public Ziplist Create()
        {
            Ziplist ret;
            return _ziplists.TryTake(out ret) ? ret : new Ziplist();
        }

        public void Add(Ziplist ziplist)
        {
            if (ziplist.SizeInMemory < LohTargetSize)
                return; // Not in LOH. Allow GC to collect.

            ziplist.Clear();
            _ziplists.Add(ziplist);
        }
    }
}