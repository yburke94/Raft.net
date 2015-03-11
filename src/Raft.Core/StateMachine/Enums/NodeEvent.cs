namespace Raft.Core.StateMachine.Enums
{
    internal enum NodeEvent
    {
        // Init Events
        NodeCreatedCluster,
        NodeJoinedCluster,

        // Log Events
        ClientScheduledCommandExecution,
        LogEntryCommited,
        CommandExecuted,

        // Cluster Events
        TermSetFromRpc,
        LeaderHearbeatTimedOut,
        CandidateElectionWon
    }
}