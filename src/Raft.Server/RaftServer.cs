using System.Threading.Tasks;
using Disruptor;

namespace Raft.Server
{
    internal class RaftServer : IRaftServer
    {
        private readonly EventPublisher<CommandScheduledEvent> _stateMachineCommandPublisher;

        public RaftServer(RingBuffer<CommandScheduledEvent> commandBuffer)
        {
            _stateMachineCommandPublisher = new EventPublisher<CommandScheduledEvent>(commandBuffer);
        }

        public Task<LogResult> Execute<T>(T command) where T : IRaftCommand, new()
        {
            var taskCompetionSource = new TaskCompletionSource<LogResult>();

            _stateMachineCommandPublisher.PublishEvent((@event, l) =>
                @event.ResetEvent(command, taskCompetionSource));

            return taskCompetionSource.Task;
        }
    }
}
