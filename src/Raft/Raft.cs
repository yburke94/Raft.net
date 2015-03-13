using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.BufferEvents;
using Raft.Server.BufferEvents.Translators;
using Raft.Server.Data;

namespace Raft
{
    internal class Raft : IRaft
    {
        private readonly IPublishToBuffer<CommandScheduled> _commandPublisher;

        public Raft(IPublishToBuffer<CommandScheduled> commandPublisher)
        {
            _commandPublisher = commandPublisher;
        }

        public Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            // TODO: Validate node state!

            var taskCompletionSource = new TaskCompletionSource<CommandExecutionResult>();
            var translator = new CommandScheduledTranslator(command, taskCompletionSource);

            _commandPublisher.PublishEvent(translator.Translate);

            return taskCompletionSource.Task;
        }
    }
}
