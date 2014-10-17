using System;
using Automatonymous;
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
        private readonly InstanceLift<NodeStateMachine> _stateMachineLift;
        private readonly INodeEvents _nodeEvents;

        public NodeStateManager(InstanceLift<NodeStateMachine> stateMachineLift, INodeEvents nodeEvents)
        {
            _stateMachineLift = stateMachineLift;
            _nodeEvents = nodeEvents;
        }

        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            var internalCommand = data.Command as IRaftInternalCommand;

            try
            {
                if (internalCommand != null)
                    _stateMachineLift.Raise(internalCommand.GetStateMachineEvent(_nodeEvents));
                else
                    _stateMachineLift.Raise(_nodeEvents.ApplyCommand);
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