using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 3 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateManager
    ///     LogVerificationManager
    ///     LogReplicationManager*
    ///     LogPersistanceManager
    /// </summary>
    internal class LogReplicationManager : IEventHandler<CommandScheduledEvent>
    {
        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            // TODO
        }
    }
}
