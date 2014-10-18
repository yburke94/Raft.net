using Stateless;

namespace Raft.Core
{
    internal class RaftNode : IRaftNode
    {
        private readonly StateMachine<NodeState, NodeEvent> _stateMachine;
        public RaftNode()
        {
            _stateMachine = new StateMachine<NodeState, NodeEvent>(NodeState.Initial);
            _stateMachine.Configure(NodeState.Initial)
                .Permit(NodeEvent.NodeJoinedCluster, NodeState.Leader);

            _stateMachine.Configure(NodeState.Leader)
                .PermitReentry(NodeEvent.ClientLoggedCommand);
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public void JoinCluster()
        {
            _stateMachine.Fire(NodeEvent.NodeJoinedCluster);
        }

        public void LogEntry()
        {
            _stateMachine.Fire(NodeEvent.ClientLoggedCommand);
        }
    }
}
