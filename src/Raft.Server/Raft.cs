using System.Threading.Tasks;
using Disruptor;
using Raft.Server.Commands;

namespace Raft.Server
{
    internal class Raft : IRaft
    {
        private readonly RaftServerContext _context;
        private readonly EventPublisher<CommandScheduledEvent> _commandPublisher;

        public Raft(EventPublisher<CommandScheduledEvent> commandPublisher, RaftServerContext context)
        {
            _context = context;
            _commandPublisher = commandPublisher;
        }

        public RaftServerContext Context
        {
            get { return _context; }
        }

        public Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            var taskCompetionSource = new TaskCompletionSource<CommandExecutionResult>();

            _commandPublisher.PublishEvent((@event, l) =>
                @event.ResetEvent(command, taskCompetionSource));

            return taskCompetionSource.Task;
        }
    }
}
