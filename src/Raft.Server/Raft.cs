using System.Threading.Tasks;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Events;

namespace Raft.Server
{
    internal class Raft : IRaft
    {
        private readonly IEventPublisher<CommandScheduled> _commandPublisher;

        public Raft(IEventPublisher<CommandScheduled> commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task<CommandExecuted> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            var taskCompletionSource = new TaskCompletionSource<CommandExecuted>();
            var translator = new CommandScheduled.Translator(command, taskCompletionSource);

            _commandPublisher.PublishEvent(translator.Translate);

            return taskCompletionSource.Task;
        }
    }
}
