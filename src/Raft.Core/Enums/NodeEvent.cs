namespace Raft.Core.Enums
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