using System;
using Stateless;

namespace Raft.Core
{
    internal class RaftNode : IRaftNode
    {
        private const int LogIncrementSize = 64;

        private readonly StateMachine<NodeState, NodeEvent> _stateMachine;
        public RaftNode()
        {
            NodeId = Guid.NewGuid();
            CurrentTerm = 0;
            LastLogIndex = 0;
            Log = new long?[LogIncrementSize];

            _stateMachine = new StateMachine<NodeState, NodeEvent>(NodeState.Initial);
            _stateMachine.Configure(NodeState.Initial)
                .Permit(NodeEvent.NodeJoinedCluster, NodeState.Leader);

            _stateMachine.Configure(NodeState.Leader)
                .PermitReentry(NodeEvent.ClientLoggedCommand);
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public Guid NodeId { get; private set; }
        public long CurrentTerm { get; private set; }
        public long LastLogIndex { get; private set; }

        public long?[] Log { get; private set; }

        public void JoinCluster()
        {
            _stateMachine.Fire(NodeEvent.NodeJoinedCluster);
        }

        public void LogEntry()
        {
            _stateMachine.Fire(NodeEvent.ClientLoggedCommand);
        }

        public void EntryLogged()
        {
            if (Log[0] != null)
                LastLogIndex++;

            if (LastLogIndex >= Log.Length)
            {
                var newLog = new long?[Log.Length + LogIncrementSize];
                Log.CopyTo(newLog, 0);
                Log = newLog;
            }

            Log[LastLogIndex] = CurrentTerm;
        }
    }
}
