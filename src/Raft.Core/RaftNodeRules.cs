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
                .Permit(NodeEvent.NodeCreatedCluster, NodeState.Leader)
                .Permit(NodeEvent.NodeJoinedCluster, NodeState.Follower);

            // Leader State Rules
            machine.Configure(NodeState.Leader)
                .PermitReentry(NodeEvent.ClientScheduledCommandExecution)
                .PermitReentry(NodeEvent.LogEntryCommited)
                .PermitReentry(NodeEvent.CommandExecuted);

            // Candidate State Rules
            machine.Configure(NodeState.Candidate)
                .Permit(NodeEvent.CandidateElectionWon, NodeState.Leader)
                .Permit(NodeEvent.TermSetFromRpc, NodeState.Follower);

            // Follower State Rules
            machine.Configure(NodeState.Follower)
                .Permit(NodeEvent.LeaderHearbeatTimedOut, NodeState.Candidate) // TODO
                .PermitReentry(NodeEvent.TermSetFromRpc)
                .PermitReentry(NodeEvent.LogEntryCommited)
                .PermitReentry(NodeEvent.CommandExecuted);
        }
    }
}
