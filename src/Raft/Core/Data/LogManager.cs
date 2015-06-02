using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raft.Core.Events;
using Raft.Infrastructure;
using Raft.Infrastructure.Compression;
using Serilog;

namespace Raft.Core.Data
{
    /// <summary>
    /// Maps log entry index to the term it was associated with.
    /// </summary>
    internal class LogManager : IHandle<TermChanged>
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
            return _indexTermMap.ContainsKey(logIdx)
                ? (long?)_indexTermMap[logIdx]
                : null;
        }

        public Ziplist GetTermLog(long term)
        {
            return _termsLog.GetTermLog(term);
        }

        public void Set(long commitIdx, long term, byte[] entry)
        {
            if (_indexTermMap.ContainsKey(commitIdx))
                throw new InvalidOperationException(
                    "An entry for this commit index has already been set." +
                    "Ensure the CommitIdx has been correctly incremented.");


        }

        public void Truncate(long idx) { }

        public void Handle(TermChanged @event)
        {
            _termsLog.StartNewTerm(@event.NewTerm);
        }
    }

    internal class TermsLog
    {
        private readonly ITermCompressionStrategy _termCompressionStrategy;
        private readonly ILogger _logger;

        private readonly IDictionary<long, byte[]> _termsLog;

        // The entry for this key in the termsLog will be de-compressed.
        private long _currentTerm = 0L;
        private readonly object _currentTermCompressionLock = new object();

        public TermsLog(ITermCompressionStrategy termCompressionStrategy, ILogger logger)
        {
            _termCompressionStrategy = termCompressionStrategy;
            _logger = logger.ForContext<TermsLog>();

            _termsLog = new Dictionary<long, byte[]>();
        }

        public Ziplist GetTermLog(long term)
        {
            if (_currentTerm <= 0)
                throw new Exception("No terms set yet in TermsLog.");

            if (!_termsLog.ContainsKey(term))
                throw new ArgumentException("No log contained for term.");

            if (term == _currentTerm)
                lock (_currentTermCompressionLock)
                    if (term == _currentTerm)
                        return Ziplist.CloneFromBytes(_termsLog[term]);

            var termLogCompressed = _termsLog[term];
            return _termCompressionStrategy.Decompress(termLogCompressed);
        }

        public void StartNewTerm(long newTerm)
        {
            if (newTerm <= 0)
                throw new ArgumentException("New Term must be greater than 0;");

            if (_currentTerm <= 0)
            {
                _termsLog[newTerm] = new Ziplist().GetBytes();
                _currentTerm = newTerm;
                return;
            }

            if (_termsLog.ContainsKey(_currentTerm))
                _logger.Warning("{Term} already existed in compressed log. " +
                                "The entry for the term will be overwritten.",
                                _currentTerm);

            lock (_currentTermCompressionLock)
            {
                var currTermLog = Ziplist.FromBytes(_termsLog[_currentTerm]);

                var compressed = _termCompressionStrategy.Compress(currTermLog);
                _termsLog[_currentTerm] = compressed;

                // We clear the old Ziplist and re-use it for the new term.
                // This to avoid LOH fragmentation as the Ziplist for the term will
                // have most likely been allocated on the LOH.
                currTermLog.Clear();

                _termsLog[newTerm] = currTermLog.GetBytes();
                _currentTerm = newTerm;
            }
        }

        private void AddEntryToCurrentTerm(byte[] entry, long term)
        {

        }
    }

    internal interface ITermCompressionStrategy
    {
        byte[] Compress(Ziplist lastTermLog);

        Ziplist Decompress(byte[] compressedBytes);
    }

    internal class SnappyTermCompressionStrategy : ITermCompressionStrategy
    {
        private readonly SnappyCompression _snappyCompression;

        public SnappyTermCompressionStrategy(SnappyCompression snappyCompression)
        {
            _snappyCompression = snappyCompression;
        }

        public byte[] Compress(Ziplist lastTermLog)
        {
            var zipListBytes = lastTermLog.GetBytes();
            return _snappyCompression.Compress(zipListBytes);
        }

        public Ziplist Decompress(byte[] compressedBytes)
        {
            var zipListBytes = _snappyCompression.Decompress(compressedBytes);
            return Ziplist.FromBytes(zipListBytes);
        }
    }
}
