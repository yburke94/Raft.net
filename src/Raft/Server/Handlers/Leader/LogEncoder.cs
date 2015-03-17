using System.IO;
using ProtoBuf;
using Raft.Core.StateMachine;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Leader
{
    /// <summary>
    /// 1 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     LogEncoder*
    ///     LogWriter
    ///     LogReplicator
    ///     CommandFinalizer
    /// </summary>
    internal class LogEncoder : LeaderEventHandler
    {
        private long _lastLogId;

        private readonly INode _node;

        public LogEncoder(INode node)
        {
            _node = node;

            _lastLogId = _node.Data.CommitIndex;
        }

        // TODO: Should add checksum for validation when sourcing from log... http://stackoverflow.com/questions/10335203/is-there-any-very-rapid-checksum-generation-algorithm
        public override void Handle(CommandScheduled @event)
        {
            var logEntry = new LogEntry {
                Term = _node.Data.CurrentTerm,
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
