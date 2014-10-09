using Disruptor;

namespace Raft.Server
{
    internal class ServerStateValidator : IEventHandler<CommandScheduledEvent>
    {
        public ServerStateValidator()
        {
              
        }

        public void OnNext(CommandScheduledEvent data, long sequence, bool endOfBatch)
        {
            
        }
    }
}