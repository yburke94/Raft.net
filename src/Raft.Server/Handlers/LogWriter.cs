using Raft.Infrastructure.Journaler;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator
    ///     LogWriter*
    ///     CommandFinalizer
    /// </summary>
    internal class LogWriter : RaftEventHandler, ISkipInternalCommands
    {
        private readonly LogRegister _logRegister;
        private readonly IJournaler _journaler;

        public LogWriter(LogRegister logRegister, IJournaler journaler)
        {
            _logRegister = logRegister;
            _journaler = journaler;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var data = _logRegister.GetEncodedLog(@event.Id);

            _journaler.WriteBlock(data);
        }
    }
}
