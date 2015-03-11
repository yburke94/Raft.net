using System;

namespace Raft.Server.Events.Data
{
    public class CommandExecuted
    {
        public CommandExecuted(bool successful) : this(successful, null) { }

        public CommandExecuted(bool successful, Exception exception)
        {
            Successful = successful;
            Exception = exception;
        }

        public bool Successful { get; private set; }

        public Exception Exception { get; set; }
    }
}