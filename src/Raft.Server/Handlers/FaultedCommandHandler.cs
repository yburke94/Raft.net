using Raft.Server.Handlers.Contracts;

namespace Raft.Server.Handlers
{
    internal class FaultedCommandHandler : RaftEventHandler, IHandleFaultedCommands
    {
        public override void Handle(CommandScheduledEvent @event)
        {
            // Todo: Handle faulted command
            // Edge case: We write to log but fail to replicate.
                // Remove from log.

            // Edge case: We write to log but fail to execute.
                // Remove from log. Somehow invalidate on replicated servers.
        }
    }
}
