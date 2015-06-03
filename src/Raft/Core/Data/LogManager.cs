using System;
using System.Collections.Generic;
using Raft.Core.Events;
using Raft.Infrastructure;

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

            _indexTermMap.Add(commitIdx, term);
            _termsLog.AddEntry(entry, term);
        }

        public void Truncate(long idx) { } // TODO

        public void Handle(TermChanged @event)
        {
            _termsLog.StartNewTerm(@event.NewTerm);
        }
    }
}
