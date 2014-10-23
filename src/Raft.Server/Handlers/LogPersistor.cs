using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     CommandEncoder
    ///     LogReplicator
    ///     LogPersistor*
    /// </summary>
    internal class LogPersistor : CommandScheduledEventHandler
    {
        public override void Handle(CommandScheduledEvent data)
        {
        //TODO
        }
    }
}
