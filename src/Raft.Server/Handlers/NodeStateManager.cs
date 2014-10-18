using System;
using Disruptor;
using Raft.Core;

namespace Raft.Server
{
    /// <summary>
    /// 1 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateManager*
    ///     LogVerificationManager
    ///     LogReplicationManager
    ///     LogPersistanceManager
    /// </summary>
    internal class NodeStateManager : IEventHandler<CommandScheduledEvent>
    {
        private readonly IRaftNode _raftNode;

        public NodeStateManager(IRaftNode raftNode)
        {
            _raftNode = raftNode;
        }

        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            if (!data.IsValidForProcessing()) return;

            var internalCommand = data.Command as IRaftInternalCommand;

            try
            {
                if (internalCommand != null)
                    internalCommand.NodeAction(_raftNode);
                else
                    _raftNode.LogEntry();
            }
            catch (Exception ex)
            {
                data.SetResult(new LogResult(false, ex));
                return;
            }
            
            data.SetResult(new LogResult(true));
        }
    }
}