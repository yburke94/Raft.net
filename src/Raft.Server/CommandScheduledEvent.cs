using System;

namespace Raft.Server
{
    internal class CommandScheduledEvent
    {
        private bool _isFaulted;
        private Action<LogResult> _setResult;

        public IRaftCommand Command { get; set; }

        public Action<LogResult> SetResult {
            get { return _setResult; }
            set {
                _setResult = result => {
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

            if (@event.SetResult == null)
                throw new ArgumentNullException("SetResult");

            Command = @event.Command;
            SetResult = @event.SetResult;
            return this;
        }

        public bool IsValidForProcessing()
        {
            return _isFaulted;
        }
    }
}
