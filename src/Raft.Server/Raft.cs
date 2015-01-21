using System.Threading.Tasks;
using Disruptor;
using Raft.Server.Commands;

namespace Raft.Server
{
    internal class Raft : IRaft
    {
        private readonly RaftServerContext _context;
        private readonly EventPublisher<CommandScheduledEvent> _stateMachineCommandPublisher;

        public Raft(RingBuffer<CommandScheduledEvent> commandBuffer, RaftServerContext context)
        {
            _context = context;
            _stateMachineCommandPublisher = new EventPublisher<CommandScheduledEvent>(commandBuffer);
        }

        public RaftServerContext Context
        {
            get { return _context; }
        }

        public Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            var taskCompetionSource = new TaskCompletionSource<CommandExecutionResult>();

            _stateMachineCommandPublisher.PublishEvent((@event, l) =>
                @event.ResetEvent(command, taskCompetionSource));

            return taskCompetionSource.Task;
        }
    }
}
