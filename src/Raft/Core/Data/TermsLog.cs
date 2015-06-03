using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raft.Infrastructure;
using Raft.Infrastructure.Compression;
using Raft.Infrastructure.Extensions;
using Serilog;

namespace Raft.Core.Data
{
    internal class TermsLog
    {
        private readonly ZiplistPool _ziplistPool;
        private readonly ICompressBlock _compressBlock;
        private readonly IDecompressBlock _decompressBlock;
        private readonly ILogger _logger;

        // TODO: Pretty sure some of these should be ConcurrentDictionary...
        private readonly IDictionary<long, byte[]> _termsLog;
        private readonly IDictionary<long, Task> _compressionTasks;

        private readonly object _compressionTasksLock = new object();

        private long _currentTerm;
        private long _lastTermAdded;

        public TermsLog(ILogger logger, ZiplistPool ziplistPool,
            ICompressBlock compressBlock, IDecompressBlock decompressBlock)
        {
            _logger = logger.ForContext<TermsLog>();
            _ziplistPool = ziplistPool;
            _compressBlock = compressBlock;
            _decompressBlock = decompressBlock;

            _termsLog = new Dictionary<long, byte[]>();
            _compressionTasks = new Dictionary<long, Task>();
        }

        public Ziplist GetTermLog(long term)
        {
            if (_currentTerm <= 0)
                throw new Exception("No terms set yet in TermsLog.");

            if (!_termsLog.ContainsKey(term))
                throw new ArgumentException("No log contained for term.");

            if (_compressionTasks.ContainsKey(term))
                lock (_compressionTasksLock)
                    if (_compressionTasks.ContainsKey(term))
                        return Ziplist.CloneFromBytes(_termsLog[term]);

            var termLogCompressed = _termsLog[term];
            return Ziplist.FromBytes(_decompressBlock.Decompress(termLogCompressed));
        }

        public void StartNewTerm(long newTerm)
        {
            if (newTerm <= 0)
                throw new ArgumentException("New Term must be greater than 0;");

            if (_currentTerm <= 0)
            {
                _termsLog[newTerm] = _ziplistPool.Create().GetBytes();
                _currentTerm = newTerm;
                return;
            }

            var currTerm = _currentTerm;

            if (_termsLog.ContainsKey(currTerm))
                _logger.Warning("{Term} already existed in compressed log. " +
                                "The entry for the term will be overwritten.",
                    currTerm);

            var compressionTask = new Task(() => Compress(currTerm));
            compressionTask.ContinueWith(
                _ =>  _compressionTasks.Remove(currTerm),
                TaskContinuationOptions.ExecuteSynchronously |
                TaskContinuationOptions.OnlyOnRanToCompletion);

            _compressionTasks.Add(currTerm, compressionTask);

            _termsLog.Add(newTerm, _ziplistPool.Create().GetBytes());
        }

        public void AddEntry(byte[] entry, long term)
        {
            if (term > _currentTerm)
                throw new ArgumentException(
                    "Term for entry is greater than current term set in Log." +
                    "Please ensure StartNewTerm() was called on the log prior to adding a new term.");

            while (term > _lastTermAdded)
            {
                if (_compressionTasks.ContainsKey(_lastTermAdded))
                    _compressionTasks[_lastTermAdded].Start();

                _lastTermAdded++;
            }

            if (term < _currentTerm && !_compressionTasks.ContainsKey(term))
                throw new InvalidOperationException(
                    "Attempting to add to a compressed term. This shouldn't have happened.");

            var termLog = Ziplist.FromBytes(_termsLog[term]);
            termLog.Push(entry);
        }

        /// <summary>
        /// Truncates the log to the specified term. Sets the specified term as the current term.
        /// Returns the Ziplist for the current term for truncating of entries.
        /// </summary>
        public Ziplist Truncate(long newCurrentTerm)
        {
            if (!_termsLog.ContainsKey(newCurrentTerm))
                throw new InvalidOperationException("The TermsLog must contain an entry for the term you wish to truncate to.");

            var termIsCompressed = newCurrentTerm != _currentTerm && !_compressionTasks.ContainsKey(newCurrentTerm);

            var termsToDelete = EnumerableUtilities.Range(newCurrentTerm+1, (int)(_currentTerm - newCurrentTerm));
            foreach (var term in termsToDelete)
            {
                // TODO: Send current term Ziplist back to pool

                if (_termsLog.ContainsKey(term))
                    _termsLog.Remove(term);

                if (_compressionTasks.ContainsKey(term))
                    _compressionTasks.Remove(term);
            }

            _currentTerm = newCurrentTerm;
            var termLog = termIsCompressed
                ? Ziplist.FromBytes(_decompressBlock.Decompress(_termsLog[newCurrentTerm]))
                : Ziplist.FromBytes(_termsLog[newCurrentTerm]);

            if (termIsCompressed)
                _termsLog[newCurrentTerm] = termLog.GetBytes();

            return termLog;
        }

        private void Compress(long term)
        {
            lock (_compressionTasksLock)
            {
                var termLog = Ziplist.FromBytes(_termsLog[term]);

                var compressed = _compressBlock.Compress(termLog.GetBytes());
                _termsLog[term] = compressed;

                _ziplistPool.Add(termLog);
            }
        }
    }
}