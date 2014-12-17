using System.IO;
using Raft.Core;
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
    internal class LogWriter : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IRaftConfiguration _raftConfiguration;
        private readonly LogRegister _logRegister;
        private readonly ILogMetadata _logMetadata;
        private readonly IWriteToFile _writeToFile;
        private readonly IMetadataFlushStrategy _metadataFlushStrategy;
        private readonly IRaftNode _raftNode;

        public LogWriter(IRaftConfiguration raftConfiguration, LogRegister logRegister, ILogMetadata logMetadata,
            IWriteToFile writeToFile, IMetadataFlushStrategy metadataFlushStrategy, IRaftNode raftNode)
        {
            _raftConfiguration = raftConfiguration;
            _logRegister = logRegister;
            _logMetadata = logMetadata;
            _writeToFile = writeToFile;
            _metadataFlushStrategy = metadataFlushStrategy;
            _raftNode = raftNode;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var data = _logRegister.GetEncodedLog(@event.Id);

            var createFile = false;
            if (_logMetadata.CurrentJournalIndex == 0 ||
                _logMetadata.NextJournalEntryOffset + data.Length > _raftConfiguration.JournalFileLength)
            {
                createFile = true;
                _logMetadata.IncrementJournalIndex();
            }

            var filePath = Path.Combine(_raftConfiguration.LogDirectory,
                string.Format("{0}.{1}", _raftConfiguration.JournalFileName, _logMetadata.CurrentJournalIndex));

            if (createFile)
                _writeToFile.CreateAndWrite(filePath, data, _raftConfiguration.JournalFileLength);
            else
                _writeToFile.Write(filePath, _logMetadata.NextJournalEntryOffset, data);

            _logMetadata.AddLogEntryToIndex(_raftNode.LastLogIndex + 1, data.Length);
            _metadataFlushStrategy.FlushLogMetadata();   
        }
    }
}
