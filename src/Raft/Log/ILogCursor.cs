using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raft.Infrastructure;

namespace Raft.Log
{
    internal interface ILogCursor
    {
        long LastTermWritten { get; }
        long LastEntryIdxWritten { get; }

        byte[] GetPreviousEntry();
        bool IsCompressed(int entryIdx);
        byte[] GetAtIdx(int entryIdx);
    }

    internal class LogCursor : ILogCursor
    {
        private readonly IDictionary<long, long> _indexTermMap;

        private readonly Ziplist[] _ziplist;

        public LogCursor(Ziplist[] ziplist, IDictionary<long, long> indexTermMap /*To Remove from constructor*/)
        {
            _ziplist = ziplist;
            _indexTermMap = indexTermMap;
        }

        public long LastTermWritten { get; private set; }
        public long LastEntryIdxWritten { get; private set; }

        public byte[] GetPreviousEntry()
        {
            return _ziplist[0].Tail().Data;
        }

        public bool IsCompressed(int entryIdx)
        {
            return false;
        }

        public byte[] GetAtIdx(int entryIdx)
        {
            throw new NotImplementedException();
        }
    }
}
