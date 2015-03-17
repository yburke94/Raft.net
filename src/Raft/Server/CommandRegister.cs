using System;
using System.Collections.Concurrent;
using System.Linq;
using Raft.Contracts;
using Raft.Core.Events;
using Raft.Infrastructure;

namespace Raft.Server
{
    /// <summary>
    /// Maintains commands for a given term so they can be lazily applied by followers.
    /// They will be held in memory until they are applied or until the term has ended.
    /// This eliminates the overhead of reading from disk and decoding the command data.
    /// </summary>
    /// <remarks>
    /// When a follower has received an entry from the leader in order to LogMatch,
    /// This should not be used. Instead the command should be applied as soon as it is received.
    /// </remarks>
    public class CommandRegister : IHandle<TermChanged>
    {
        private readonly ConcurrentDictionary<EntryKey, IRaftCommand> _raftCommands = new ConcurrentDictionary<EntryKey, IRaftCommand>();

        public void Add(long term, long commandIdx, IRaftCommand command)
        {
            var key = new EntryKey(term, commandIdx);
            if (!_raftCommands.TryAdd(key, command))
                throw new ApplicationException("Failed to add entry for command.");
        }

        public IRaftCommand Get(long term, long commandIdx)
        {
            IRaftCommand command;
            _raftCommands.TryGetValue(new EntryKey(term, commandIdx), out command);
            return command;
        }

        public void Handle(TermChanged @event)
        {
            const int removalAttempts = 5;
            _raftCommands
                .Where(x => x.Key.Term != @event.NewTerm)
                .Select(x => x.Key).ToList()
                .ForEach(x => {
                    var attempts = 0;
                    IRaftCommand discarded;
                    while (!_raftCommands.TryRemove(x, out discarded) && attempts < removalAttempts)
                        attempts++;
                });
        }

        private class EntryKey
        {
            public long Term { get; private set; }
            public long LogIdx { get; private set; }

            public EntryKey(long term, long logIdx)
            {
                Term = term;
                LogIdx = logIdx;
            }

            public override bool Equals(object obj)
            {
                var key = obj as EntryKey;
                return key != null && key.Term.Equals(Term) && key.LogIdx.Equals(LogIdx);
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    var hash = (int)2166136261;
                    hash = hash * 16777619 ^ Term.GetHashCode();
                    hash = hash * 16777619 ^ LogIdx.GetHashCode();
                    return hash;
                }
            }
        }
    }
}
