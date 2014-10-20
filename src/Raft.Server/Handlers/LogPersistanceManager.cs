using System.Linq;
using Disruptor;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 4 of 4 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateManager
    ///     LogVerificationManager
    ///     LogReplicationManager
    ///     LogPersistanceManager*
    /// </summary>
    internal class LogPersistanceManager : IEventHandler<CommandScheduledEvent>
    {
        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            if (!data.IsValidForProcessing()) return; // TODO: DRY

            if (data.Command is IRaftInternalCommand) return;


        }
    }
}
