using System;
using Raft.Core.Enums;
using Raft.Core.Events;
using Raft.Infrastructure;
using Stateless;

namespace Raft.Core
{
    // TODO: This is shared state which i don't like! Have the state machine managed by a seperate thread which is sent messages.
    internal class RaftNode : IRaftNode
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly StateMachine<NodeState, NodeEvent> _stateMachine;

        public RaftNode(IEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
            NodeId = Guid.NewGuid();
            CurrentTerm = 0;
            CommitIndex = 0;
            LastApplied = 0;
            Log = new RaftLog();

            _stateMachine = new StateMachine<NodeState, NodeEvent>(NodeState.Initial);
            _stateMachine.ApplyRaftRulesToStateMachine();
        }

        public NodeState CurrentState {
            get { return _stateMachine.State; }
        }

        public Guid NodeId { get; private set; }

        /// <summary>
        /// Latest term server has seen
        /// </summary>
        /// <remarks>initialized to 0 on first boot, increases monotonically</remarks>
        public long CurrentTerm { get; private set; }

        /// <summary>
        /// Index of highest log entry known to be committed
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically.</remarks>
        public long CommitIndex { get; private set; }

        /// <summary>
        /// Index of highest log entry applied to state machine
        /// </summary>
        /// <remarks>Initialized to 0, increases monotonically</remarks>
        public long LastApplied { get; private set; }

        public RaftLog Log { get; private set; }

        public void CreateCluster()
        {
            _stateMachine.Fire(NodeEvent.NodeCreatedCluster);
        }

        public void JoinCluster()
        {
            _stateMachine.Fire(NodeEvent.NodeJoinedCluster);
        }

        public void ScheduleCommandExecution()
        {
            _stateMachine.Fire(NodeEvent.ClientScheduledCommandExecution);
        }

        public void CommitLogEntry(long entryIdx, long term)
        {
            if (term > CurrentTerm)
                throw new InvalidOperationException("Cannot commit a log entry against a term greater than the current term.");

            _stateMachine.Fire(NodeEvent.LogEntryCommited);

            CommitIndex = Math.Max(CommitIndex, entryIdx);

            Log.SetLogEntry(entryIdx, term);
        }

        public void ApplyCommand(long entryIdx)
        {
            _stateMachine.Fire(NodeEvent.CommandExecuted);
            LastApplied = Math.Max(LastApplied, entryIdx);
        }

        public void SetTermFromRpc(long term)
        {
            if (term < CurrentTerm)
                throw new InvalidOperationException(string.Format(
                    "The current term for this node was: {0}." +
                    "An attempt was made to set the term for this node to: {1}." +
                    "The node must only ever increment their term.", CurrentTerm, term));

            _stateMachine.Fire((NodeEvent.TermSetFromRpc));
            CurrentTerm = term;
            _eventDispatcher.Publish(new TermChanged(CurrentTerm));
        }

        public void TimeoutLeaderHeartbeat()
        {
            _stateMachine.Fire(NodeEvent.LeaderHearbeatTimedOut);
            CurrentTerm++;
            _eventDispatcher.Publish(new TermChanged(CurrentTerm));
        }

        public void WinCandidateElection()
        {
            _stateMachine.Fire(NodeEvent.CandidateElectionWon);
        }
    }
}
