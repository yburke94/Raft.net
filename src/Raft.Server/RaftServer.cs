using Disruptor;
using Raft.Infrastructure;

namespace Raft.Server
{
    internal class RaftServer : IRaftServer
    {
        private readonly EventPublisher<CommandScheduledEvent> _stateMachineCommandPublisher;

        public RaftServer(RingBuffer<CommandScheduledEvent> commandBuffer)
        {
            _stateMachineCommandPublisher = new EventPublisher<CommandScheduledEvent>(commandBuffer);
        }

        public IFuture<LogResult> Execute<T>(T command) where T : IRaftCommand, new()
        {
            var future = new TwoPhaseWaitFuture<LogResult>();

            _stateMachineCommandPublisher.PublishEvent((@event, l) =>
                @event.Copy(new CommandScheduledEvent {
                    Command = command,
                    SetResult = future.SetResult
                }));

            return future; // Ask again later...
        }
    }
}
