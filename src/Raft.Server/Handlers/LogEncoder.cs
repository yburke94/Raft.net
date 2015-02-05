﻿using System.IO;
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
        private long _lastLogId;

        private readonly IRaftNode _raftNode;
        private readonly LogEntryRegister _logEntryRegister;

        public LogEncoder(IRaftNode raftNode, LogEntryRegister logEntryRegister)
        {
            _raftNode = raftNode;
            _logEntryRegister = logEntryRegister;

            _lastLogId = _raftNode.CommitIndex;
        }

        // TODO: Should add checksum for validation when sourcing from log... http://stackoverflow.com/questions/10335203/is-there-any-very-rapid-checksum-generation-algorithm
        public override void Handle(CommandScheduledEvent @event)
        {
            var logEntry = new LogEntry {
                Term = _raftNode.CurrentTerm,
                Index = _lastLogId + 1,
                CommandType = @event.Command.GetType().AssemblyQualifiedName,
                Command = @event.Command
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, logEntry);
                _logEntryRegister.AddEncodedLog(@event.Id, logEntry.Index, ms.ToArray());
            }

            _lastLogId++;
        }
    }
}
