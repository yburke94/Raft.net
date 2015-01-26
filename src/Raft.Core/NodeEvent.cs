namespace Raft.Core
{
    internal enum NodeEvent
    {
        NodeCreatedCluster,
        ClientScheduledCommandExecution,
        LogEntryAdded,
        CommandExecuted,
        HigherTermSet
    }
}