using System;
using System.Collections.Generic;
using Raft.Infrastructure;
using Raft.Infrastructure.Compression;

namespace Raft.Core.Data
{
    /// <summary>
    /// Maps log entry index to the term it was associated with.
    /// </summary>
    internal class LogManager
    {
        private readonly IDictionary<long, long> _indexTermMap;

        private readonly TermsLog _termsLog;

        public LogManager(TermsLog termsLog)
        {
            _termsLog = termsLog;
            _indexTermMap = new Dictionary<long, long>();
        }

        public long? GetTerm(long logIdx)
        {
            return null;
        }

        public ZipList GetTermLog(long term)
        {
            return null;
        }

        public void Set(long commitIdx, long term, byte[] entry) { }

        public void Truncate(long idx) { }
    }

    internal class TermsLog
    {
        private const int TermsIncrementSize = 64;

        private readonly ITermCompressionStrategy _termCompressionStrategy;
        private readonly byte[][] _compressedTerms;

        public TermsLog(ITermCompressionStrategy termCompressionStrategy)
        {
            _termCompressionStrategy = termCompressionStrategy;
            _compressedTerms = new byte[TermsIncrementSize][];
        }

        public ZipList GetTermLog(long term)
        {
            if (term >= _compressedTerms.Length)
                throw new IndexOutOfRangeException(
                    "Term was larger than the amount of compressed term logs stored.");

            var termLogCompressed = _compressedTerms[term];
            return _termCompressionStrategy.Decompress(termLogCompressed);
        }

        // TODO: Handle expanding and checking for memory limits exceeded.
        // TODO: Consider compacting the LOH if compressed is too large.
        public void PushNewTerm(ZipList termLog, long term)
        {
            var compressed = _termCompressionStrategy.Compress(termLog);
            _compressedTerms[term] = compressed;
        }
    }

    internal interface ITermCompressionStrategy
    {
        byte[] Compress(ZipList lastTermLog);

        ZipList Decompress(byte[] compressedBytes);
    }

    internal class SnappyTermCompressionStrategy : ITermCompressionStrategy
    {
        private readonly SnappyCompression _snappyCompression;

        public SnappyTermCompressionStrategy(SnappyCompression snappyCompression)
        {
            _snappyCompression = snappyCompression;
        }

        public byte[] Compress(ZipList lastTermLog)
        {
            var zipListBytes = lastTermLog.GetBytes();
            return _snappyCompression.Compress(zipListBytes);
        }

        public ZipList Decompress(byte[] compressedBytes)
        {
            var zipListBytes = _snappyCompression.Decompress(compressedBytes);
            return ZipList.FromBytes(zipListBytes);
        }
    }
}
