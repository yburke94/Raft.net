using System.Threading.Tasks;
using Raft.Contracts;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Enums;
using Raft.Exceptions;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.BufferEvents;
using Raft.Server.BufferEvents.Translators;
using Raft.Server.Data;

namespace Raft
{
    internal class RaftApi : IRaft
    {
        private readonly IPublishToBuffer<CommandScheduled> _commandPublisher;
        private readonly IRaftNode _node;

        public RaftApi(IPublishToBuffer<CommandScheduled> commandPublisher, IRaftNode node)
        {
            _commandPublisher = commandPublisher;
            _node = node;
        }

        public Task<CommandExecutionResult> ExecuteCommand<T>(T command) where T : IRaftCommand, new()
        {
            if (_node.CurrentState != NodeState.Leader)
                throw new NotClusterLeaderException();

            var taskCompletionSource = new TaskCompletionSource<CommandExecutionResult>();
            var translator = new CommandScheduledTranslator(command, taskCompletionSource);

            _commandPublisher.PublishEvent(translator.Translate);

            return taskCompletionSource.Task;
        }

        public string GetClusterLeader()
        {
            // TODO: Expose method to get cluster leader.
            return string.Empty;
        }
    }
}
