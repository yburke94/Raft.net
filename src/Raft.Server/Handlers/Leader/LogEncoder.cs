using System.IO;
using ProtoBuf;
using Raft.Core;
using Raft.Server.Events;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;
using Raft.Server.Registers;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 2 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder*
    ///     LogWriter
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    internal class LogEncoder : LeaderEventHandler, ISkipInternalCommands
    {
        private long _lastLogId;

        private readonly IRaftNode _raftNode;

        public LogEncoder(IRaftNode raftNode)
        {
            _raftNode = raftNode;

            _lastLogId = _raftNode.CommitIndex;
        }

        // TODO: Should add checksum for validation when sourcing from log... http://stackoverflow.com/questions/10335203/is-there-any-very-rapid-checksum-generation-algorithm
        public override void Handle(CommandScheduled @event)
        {
            var logEntry = new LogEntry {
                Term = _raftNode.CurrentTerm,
                Index = _lastLogId + 1,
                CommandType = @event.Command.GetType().AssemblyQualifiedName,
                Command = @event.Command
            };

            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, logEntry, PrefixStyle.Base128);
                @event.SetLogEntry(logEntry, ms.ToArray());
            }

            _lastLogId++;
        }
    }
}
