using System;
using Raft.Core.Commands;
using Raft.Core.Events;
using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;
using Raft.Infrastructure;
using Stateless;

namespace Raft.Core.StateMachine
{
    // TODO: This is shared state which i don't like! Have the state machine managed by a seperate thread which is sent messages.
    // TODO: Handle server state that must be persisted.
    internal class Node : INode,
        IHandle<CreateCluster>,
        IHandle<JoinCluster>,
        IHandle<CommitEntry>,
        IHandle<ApplyEntry>,
        IHandle<WinCandidateElection>,
        IHandle<SetNewTerm>,
        IHandle<TimeoutLeaderHeartbeat>
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly StateMachine<NodeState, Type> _stateMachine;

        public Node(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            
            _stateMachine = new StateMachine<NodeState, Type>(NodeState.Initial);
            _stateMachine.ApplyRaftRulesToStateMachine();

            Data = new NodeData();
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public NodeData Data { get; private set; }

        public void FireAtStateMachine<T>() where T : INodeCommand
        {
            _stateMachine.Fire(typeof(T));
        }

        public void Handle(CreateCluster @event)
        {
            // TODO
        }

        public void Handle(JoinCluster @event)
        {
            // TODO
        }

        public void Handle(CommitEntry @event)
        {
            if (@event.EntryTerm > Data.CurrentTerm)
                throw new InvalidOperationException("Cannot commit a log entry against a term greater than the current term.");

            Data.CommitIndex = Math.Max(Data.CommitIndex, @event.EntryIdx);

            Data.Log.SetLogEntry(@event.EntryIdx, @event.EntryTerm);
        }

        public void Handle(ApplyEntry @event)
        {
            Data.LastApplied = Math.Max(Data.LastApplied, @event.EntryIdx);
        }

        public void Handle(WinCandidateElection @event)
        {
            // TODO
        }

        public void Handle(SetNewTerm @event)
        {
            if (@event.Term < Data.CurrentTerm)
                throw new InvalidOperationException(string.Format(
                    "The current term for this node was: {0}." +
                    "An attempt was made to set the term for this node to: {1}." +
                    "The node must only ever increment their term.", Data.CurrentTerm, @event.Term));

            Data.CurrentTerm = @event.Term;
            _eventDispatcher.Publish(new TermChanged(Data.CurrentTerm));
        }

        public void Handle(TimeoutLeaderHeartbeat @event)
        {
            Data.CurrentTerm++;
            _eventDispatcher.Publish(new TermChanged(Data.CurrentTerm));
        }
    }
}
