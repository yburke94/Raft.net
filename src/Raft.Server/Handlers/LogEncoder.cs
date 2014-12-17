using System.IO;
using ProtoBuf;
using Raft.Core;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 2 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder*
    ///     LogReplicator
    ///     LogWriter
    /// </summary>
    internal class LogEncoder : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IRaftNode _raftNode;
        private readonly LogRegister _logRegister;

        public LogEncoder(IRaftNode raftNode, LogRegister logRegister)
        {
            _raftNode = raftNode;
            _logRegister = logRegister;
        }

        // TODO: Should prepend checksum... http://stackoverflow.com/questions/10335203/is-there-any-very-rapid-checksum-generation-algorithm
        public override void Handle(CommandScheduledEvent @event)
        {
            var logEntry = new LogEntry {
                Term = _raftNode.CurrentLogTerm,
                Index = _raftNode.LastLogIndex + 1,
                CommandType = @event.Command.GetType().AssemblyQualifiedName,
                Command = @event.Command
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, logEntry);
                _logRegister.AddEncodedLog(@event.Id, ms.ToArray());
            }
        }
    }
}
