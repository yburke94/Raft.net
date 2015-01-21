using System;
using Stateless;

namespace Raft.Core
{
    internal class RaftNode : IRaftNode
    {
        private readonly StateMachine<NodeState, NodeEvent> _stateMachine;

        public RaftNode()
        {
            NodeId = Guid.NewGuid();
            CurrentTerm = 0;
            CommitIndex = 0;
            LastApplied = 0;
            Log = new RaftLog();

            _stateMachine = new StateMachine<NodeState, NodeEvent>(NodeState.Initial);
            _stateMachine.Configure(NodeState.Initial)
                .Permit(NodeEvent.NodeCreatedCluster, NodeState.Leader);

            _stateMachine.Configure(NodeState.Leader)
                .PermitReentry(NodeEvent.ClientScheduledCommandExecution)
                .PermitReentry(NodeEvent.LogEntryAdded)
                .PermitReentry(NodeEvent.CommandExecuted);
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public Guid NodeId { get; private set; }

        /// <summary>
        /// Latest term server has seen
        /// </summary>
        /// <remarks>initialized to 0 on first boot, increases monotonically</remarks>
        public long CurrentTerm { get; private set; }

        /// <summary>
        /// Index of highest log entry known to be committed
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically.</remarks>
        public long CommitIndex { get; private set; }

        /// <summary>
        /// Index of highest log entry applied to state machine
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically</remarks>
        public long LastApplied { get; private set; }

        public RaftLog Log { get; private set; }

        public void CreateCluster()
        {
            _stateMachine.Fire(NodeEvent.NodeCreatedCluster);
        }

        public void ScheduleCommandExecution()
        {
            _stateMachine.Fire(NodeEvent.ClientScheduledCommandExecution);
        }

        public void AddLogEntry()
        {
            _stateMachine.Fire(NodeEvent.LogEntryAdded);

            CommitIndex++;
            Log.SetLogEntry(CommitIndex, CurrentTerm);
        }

        public void ApplyCommand()
        {
            _stateMachine.Fire(NodeEvent.CommandExecuted);
            LastApplied++;
        }
    }
}
