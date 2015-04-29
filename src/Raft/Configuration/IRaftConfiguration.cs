using System;
using System.ServiceModel.Channels;
using Raft.Contracts.Persistance;
using Serilog;

namespace Raft.Configuration
{
    public interface IRaftConfiguration {
        // Persistance
        Func<IWriteDataBlocks> GetBlockWriter { get; set; }
        Func<IReadDataBlocks> GetBlockReader { get; set; }

        Binding RaftServiceBinding { get; set; }
        Func<ILogger> GetLogger { get; set; }
    }
}