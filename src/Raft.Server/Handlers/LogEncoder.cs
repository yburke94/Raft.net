﻿using System.IO;
using System.Linq;
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
    ///     LogPersistor
    /// </summary>
    internal class LogEncoder : CommandScheduledEventHandler
    {
        private readonly IRaftNode _raftNode;

        public LogEncoder(IRaftNode raftNode)
            : base(skipInternalCommands:true)
        {
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent data)
        {
            var logEntry = new LogEntry {
                Term = _raftNode.CurrentLogTerm,
                Index = _raftNode.LastLogIndex + 1,
                CommandType = data.Command.GetType().AssemblyQualifiedName,
                Command = data.Command
            };

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, logEntry);
                data.Metadata["EncodedLog"] = ms.ToArray();
            }
        }
    }
}
