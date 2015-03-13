using Raft.Core.StateMachine.Enums;
using Stateless;

namespace Raft.Core.StateMachine
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
                .Ignore(NodeEvent.ClientScheduledCommandExecution)
                .Ignore(NodeEvent.LogEntryCommited)
                .Ignore(NodeEvent.CommandExecuted);

            // Candidate State Rules
            machine.Configure(NodeState.Candidate)
                .Permit(NodeEvent.CandidateElectionWon, NodeState.Leader)
                .Permit(NodeEvent.TermSetFromRpc, NodeState.Follower);

            // Follower State Rules
            machine.Configure(NodeState.Follower)
                .Permit(NodeEvent.LeaderHearbeatTimedOut, NodeState.Candidate) // TODO
                .Ignore(NodeEvent.TermSetFromRpc)
                .Ignore(NodeEvent.LogEntryCommited)
                .Ignore(NodeEvent.CommandExecuted);
        }


    }
}
