using Raft.Core.Enums;
using Stateless;

namespace Raft.Core
{
    internal static class RaftNodeRules
    {
        public static void ApplyRaftRulesToStateMachine(this StateMachine<NodeState, NodeEvent> machine)
        {
            // Initial State Rules
            machine.Configure(NodeState.Initial)
                .Permit(NodeEvent.NodeCreatedCluster, NodeState.Leader);

            // Leader State Rules
            machine.Configure(NodeState.Leader)
                .PermitReentry(NodeEvent.ClientScheduledCommandExecution)
                .PermitReentry(NodeEvent.LogEntryAdded)
                .PermitReentry(NodeEvent.CommandExecuted)
                .Permit(NodeEvent.HigherTermSet, NodeState.Follower);

            // Candidate State Rules
            machine.Configure(NodeState.Candidate)
                .Permit(NodeEvent.HigherTermSet, NodeState.Follower);

            // Follower State Rules
            machine.Configure(NodeState.Follower)
                .PermitReentry(NodeEvent.HigherTermSet);
        }
    }
}
