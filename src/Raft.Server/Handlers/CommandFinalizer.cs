using System.Linq;

namespace Raft.Server.Handlers
{
    internal class CommandFinalizer : RaftEventHandler
    {
        private readonly LogRegister _logRegister;

        public CommandFinalizer(LogRegister logRegister)
        {
            _logRegister = logRegister;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            if (@event.TaskCompletionSource != null)
                @event.TaskCompletionSource.SetResult(new LogResult(true));

            if (_logRegister.HasLogEntry(@event.Id))
                _logRegister.EvictEntry(@event.Id);
        }
    }
}
