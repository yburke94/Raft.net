using System.Linq;

namespace Raft.Server.Handlers
{
    internal class CommandExecutor : RaftEventHandler, ISkipInternalCommands
    {
        public override void Handle(CommandScheduledEvent @event)
        {
            @event.Command.Execute(null);
        }
    }
}
