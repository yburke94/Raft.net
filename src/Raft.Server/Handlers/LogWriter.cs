using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Raft.Infrastructure;
using Raft.Infrastructure.IO;
using Raft.Server.Configuration;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter*
    /// </summary>
    internal class LogWriter : CommandScheduledEventHandler
    {
        private readonly IRaftConfiguration _raftConfiguration;
        private readonly LogRegister _logRegister;
        private readonly IDiskWriteStrategy _diskWriteStrategy;

        public LogWriter(IRaftConfiguration raftConfiguration, LogRegister logRegister,
            IDiskWriteStrategy diskWriteStrategy)
        {
            _raftConfiguration = raftConfiguration;
            _logRegister = logRegister;
            _diskWriteStrategy = diskWriteStrategy;
        }

        public override bool SkipInternalCommands
        {
            get { return true; }
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var logPath = _raftConfiguration.LogPath;
            var bytes = _logRegister.GetEncodedLog(@event.Id);

            _diskWriteStrategy.Write(logPath, bytes);
        }
    }
}
