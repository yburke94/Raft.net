using System;
using System.Collections.Generic;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        private bool _isFaulted;
        private Action<LogResult> _whenLogged;

        public IDictionary<string, object> Metadata { get; set; }

        public IRaftCommand Command { get; set; }

        public Action<LogResult> WhenLogged {
            get { return _whenLogged; }
            set {
                _whenLogged = result => {
                    if (!result.Successful)
                        _isFaulted = true;

                    value(result);
                };
            }
        }

        public CommandScheduledEvent Copy(CommandScheduledEvent @event)
        {
            if (@event.Command == null)
                throw new ArgumentNullException("Command");

            if (@event.WhenLogged == null && !(@event.Command is IRaftInternalCommand))
                throw new ArgumentNullException("SetResult");

            Command = @event.Command;
            WhenLogged = @event.WhenLogged;
            Metadata = new Dictionary<string, object>();
            return this;
        }

        public bool IsValidForProcessing()
        {
            return !_isFaulted;
        }
    }
}
