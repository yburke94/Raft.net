using System;
using Raft.Core.Commands;
using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;
using Stateless;

namespace Raft.Core.StateMachine
{
    internal static class NodeRules
    {
        public static void ApplyRaftRulesToStateMachine(this StateMachine<NodeState, Type> machine, NodeProperties nodeProperties)
        {
            // Initial State Rules
            machine.Configure(NodeState.Initial)
                .Permit(typeof(CreateCluster), NodeState.Leader)
                .Permit(typeof(JoinCluster), NodeState.Follower);

            // Leader State Rules
            machine.Configure(NodeState.Leader)
                .OnEntry(() => nodeProperties.LeaderId = Guid.Empty)
                .Permit(typeof(SetNewTerm), NodeState.Follower)
                .Ignore(typeof(CommitEntry))
                .Ignore(typeof(ApplyEntry));

            // Candidate State Rules
            machine.Configure(NodeState.Candidate)
                .OnEntry(() => nodeProperties.LeaderId = Guid.Empty)
                .Permit(typeof(WinCandidateElection), NodeState.Leader)
                .Permit(typeof(SetNewTerm), NodeState.Follower)
                .Permit(typeof(CancelElection), NodeState.Follower);

            // Follower State Rules
            machine.Configure(NodeState.Follower)
                .Permit(typeof(TimeoutLeaderHeartbeat), NodeState.Candidate) // TODO: Impl
                .Ignore(typeof(SetNewTerm))
                .Ignore(typeof(CommitEntry))
                .Ignore(typeof(ApplyEntry))
                .Ignore(typeof(SetLeaderInformation))
                .Ignore(typeof(TruncateLog));
        }
    }
}
