using System;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        public IRaftCommand Command { get; set; }

        public Action<LogResult> SetResult { get; set; }

        public CommandScheduledEvent Copy(CommandScheduledEvent @event)
        {
            if (@event.Command == null)
                throw new ArgumentNullException("Command");

            if (@event.SetResult == null)
                throw new ArgumentNullException("SetResult");

            Command = @event.Command;
            SetResult = @event.SetResult;
            return this;
        }
    }
}
