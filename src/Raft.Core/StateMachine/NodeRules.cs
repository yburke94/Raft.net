using System;
using Raft.Core.Commands;
using Raft.Core.StateMachine.Enums;
using Stateless;

namespace Raft.Core.StateMachine
{
    internal static class NodeRules
    {
        public static void ApplyRaftRulesToStateMachine(this StateMachine<NodeState, Type> machine)
        {
            // Initial State Rules
            machine.Configure(NodeState.Initial)
                .Permit(typeof(CreateCluster), NodeState.Leader)
                .Permit(typeof(JoinCluster), NodeState.Follower);

            // Leader State Rules
            machine.Configure(NodeState.Leader)
                .Ignore(typeof(CommitEntry))
                .Ignore(typeof(ApplyEntry));

            // Candidate State Rules
            machine.Configure(NodeState.Candidate)
                .Permit(typeof(WinCandidateElection), NodeState.Leader)
                .Permit(typeof(SetNewTerm), NodeState.Follower);

            // Follower State Rules
            machine.Configure(NodeState.Follower)
                .Permit(typeof(TimeoutLeaderHeartbeat), NodeState.Candidate) // TODO
                .Ignore(typeof(SetNewTerm))
                .Ignore(typeof(CommitEntry))
                .Ignore(typeof(ApplyEntry));
        }

        
    }
}
