using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 2 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateManager
    ///     LogVerificationManager*
    ///     LogReplicationManager
    ///     LogPersistanceManager
    /// </summary>
    internal class LogVerificationManager : IEventHandler<CommandScheduledEvent>
    {
        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            // TODO
        }
    }
}
