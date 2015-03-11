using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Infrastructure.Disruptor;
using Raft.Server.Commands;
using Raft.Server.Events;
using Raft.Server.Events.Data;
using Raft.Server.Events.Translators;

namespace Raft
{
    internal class Raft : IRaft
    {
        private readonly IPublishToBuffer<CommandScheduled> _commandPublisher;

        public Raft(IPublishToBuffer<CommandScheduled> commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task<CommandExecuted> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            var taskCompletionSource = new TaskCompletionSource<CommandExecuted>();
            var translator = new CommandScheduledTranslator(command, taskCompletionSource);

            _commandPublisher.PublishEvent(translator.Translate);

            return taskCompletionSource.Task;
        }
    }
}
