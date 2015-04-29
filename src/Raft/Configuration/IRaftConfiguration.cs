using System;
using System.ServiceModel.Channels;
using Raft.Contracts.Persistance;
using Serilog;

namespace Raft.Configuration
{
    public interface IRaftConfiguration {
        // Persistance
        Func<IWriteDataBlocks> GetBlockWriter { get; }
        Func<IReadDataBlocks> GetBlockReader { get; }

        Binding RaftServiceBinding { get; }
        Func<ILogger> GetLogger { get; }
    }
}