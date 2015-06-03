using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Raft.Infrastructure;
using Serilog;

namespace Raft.Core.Data
{
    internal class TermsLog
    {
        private readonly ITermCompressionStrategy _termCompressionStrategy;
        private readonly ZiplistPool _ziplistPool;
        private readonly ILogger _logger;

        // TODO: Pretty sure some of these should be ConcurrentDictionary...
        private readonly IDictionary<long, byte[]> _termsLog;
        private readonly IDictionary<long, Task> _compressionTasks;

        private readonly object _compressionTasksLock = new object();

        private long _currentTerm = 0L;
        private long _lastTermAdded = 0L;

        public TermsLog(ITermCompressionStrategy termCompressionStrategy, ILogger logger, ZiplistPool ziplistPool)
        {
            _termCompressionStrategy = termCompressionStrategy;
            _ziplistPool = ziplistPool;
            _logger = logger.ForContext<TermsLog>();

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
            return _termCompressionStrategy.Decompress(termLogCompressed);
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

            if (_lastTermAdded < term)
            {
                while (_lastTermAdded != term)
                {
                    if (_compressionTasks.ContainsKey(_lastTermAdded))
                        _compressionTasks[_lastTermAdded].Start();

                    _lastTermAdded++;
                }
            }

            if (term < _currentTerm && !_compressionTasks.ContainsKey(term))
                throw new InvalidOperationException(
                    "Attempting to add to a compressed term. This shouldn't have happened.");

            var termLog = Ziplist.FromBytes(_termsLog[term]);
            termLog.Push(entry);
        }

        private void Compress(long term)
        {
            lock (_compressionTasksLock)
            {
                var termLog = Ziplist.FromBytes(_termsLog[term]);

                var compressed = _termCompressionStrategy.Compress(termLog);
                _termsLog[term] = compressed;

                _ziplistPool.Add(termLog);
            }
        }
    }
}