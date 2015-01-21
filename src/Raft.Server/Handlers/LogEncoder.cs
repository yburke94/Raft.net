using System.IO;
using ProtoBuf;
using Raft.Core;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 2 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder*
    ///     LogWriter
    ///     LogReplicator
    ///     CommandApplier
    /// </summary>
    internal class LogEncoder : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IRaftNode _raftNode;
        private readonly EncodedLogRegister _encodedLogRegister;

        public LogEncoder(IRaftNode raftNode, EncodedLogRegister encodedLogRegister)
        {
            _raftNode = raftNode;
            _encodedLogRegister = encodedLogRegister;
        }

        // TODO: Should add checksum for validation when sourcing from log... http://stackoverflow.com/questions/10335203/is-there-any-very-rapid-checksum-generation-algorithm
        public override void Handle(CommandScheduledEvent @event)
        {
            var logEntry = new LogEntry {
                Term = _raftNode.CurrentTerm,
                Index = _raftNode.CommitIndex + 1,
                CommandType = @event.Command.GetType().AssemblyQualifiedName,
                Command = @event.Command
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, logEntry);
                _encodedLogRegister.AddEncodedLog(@event.Id, ms.ToArray());
            }
        }
    }
}
