using System;
using Raft.Core.Commands;
using Raft.Core.Events;
using Raft.Core.StateMachine.Data;
using Raft.Core.StateMachine.Enums;
using Raft.Infrastructure;
using Stateless;

namespace Raft.Core.StateMachine
{
    // TODO: Handle server state that must be persisted.
    internal class Node : INode,
        IHandle<CreateCluster>,
        IHandle<JoinCluster>,
        IHandle<CommitEntry>,
        IHandle<ApplyEntry>,
        IHandle<WinCandidateElection>,
        IHandle<SetNewTerm>,
        IHandle<TimeoutLeaderHeartbeat>,
        IHandle<SetLeaderInformation>,
        IHandle<TruncateLog>
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly StateMachine<NodeState, Type> _stateMachine;

        public Node(IEventDispatcher eventDispatcher)
        {
            Properties = new NodeProperties();
            Log = new InMemoryLog();

            _eventDispatcher = eventDispatcher;
            _stateMachine = new StateMachine<NodeState, Type>(NodeState.Initial);

            _stateMachine.ApplyRaftRulesToStateMachine(Properties);
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public NodeProperties Properties { get; private set; }

        public InMemoryLog Log { get; private set; }

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
            if (@event.EntryTerm > Properties.CurrentTerm)
                throw new InvalidOperationException("Cannot commit a log entry against a term greater than the current term.");

            Properties.CommitIndex = Math.Max(Properties.CommitIndex, @event.EntryIdx);

            Log.SetLogEntry(@event.EntryIdx, @event.EntryTerm);
        }

        public void Handle(ApplyEntry @event)
        {
            Properties.LastApplied = Math.Max(Properties.LastApplied, @event.EntryIdx);
        }

        public void Handle(WinCandidateElection @event)
        {
            // TODO
        }

        public void Handle(SetNewTerm @event)
        {
            if (@event.Term < Properties.CurrentTerm)
                throw new InvalidOperationException(string.Format(
                    "The current term for this node was: {0}." +
                    "An attempt was made to set the term for this node to: {1}." +
                    "The node must only ever increment their term.", Properties.CurrentTerm, @event.Term));

            Properties.CurrentTerm = @event.Term;
            _eventDispatcher.Publish(new TermChanged(Properties.CurrentTerm));
        }

        public void Handle(TimeoutLeaderHeartbeat @event)
        {
            Properties.CurrentTerm++;
            _eventDispatcher.Publish(new TermChanged(Properties.CurrentTerm));
        }

        public void Handle(SetLeaderInformation @event)
        {
            Properties.LeaderId = @event.LeaderId;
        }

        public void Handle(TruncateLog @event)
        {
            if (@event.TruncateFromIndex > Properties.CommitIndex)
                throw new InvalidOperationException("Cannot truncate from the specified index as it is greater than the nodes current commit index.");

            Properties.CommitIndex = @event.TruncateFromIndex;
            Properties.LastApplied = @event.TruncateFromIndex;

            Log.TruncateLog(@event.TruncateFromIndex);
        }
    }
}
