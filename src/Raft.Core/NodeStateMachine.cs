using System;
using Automatonymous;
using Raft.Core.CommandLogging;

namespace Raft.Core
{
    public class NodeStateMachine : AutomatonymousStateMachine<NodeState>, INodeEvents
    {
        public NodeStateMachine()
        {
            State(() => Follower);
            State(() => Leader);
            State(() => Candidate);

            Event(() => JoinCluster);
            Event(() => ApplyCommand);

            Initially(
                When(JoinCluster)
                .TransitionTo(Leader));

            During(Leader,
                When(ApplyCommand)
                    .Then(state => state.ApplyCommandStrategy = new PersistLogCommandToDisk()),
                When(JoinCluster)
                    .Then(x => { throw new Exception();})
            );
        }

        public State Follower { get; set; }

        public State Leader { get; set; }

        public State Candidate { get; set; }

        public Event JoinCluster { get; set; }

        public Event ApplyCommand { get; set; }
    }
}
