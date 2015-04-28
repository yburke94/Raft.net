using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Serilog;

namespace Raft.Server.Handlers.Core
{
    internal class NodeCommandExecutor : BufferEventHandler<InternalCommandScheduled>
    {
        private readonly Node _node;
        private readonly ILogger _logger;

        public NodeCommandExecutor(Node node, ILogger logger)
        {
            _node = node;
            _logger = logger.ForContext("BufferHandler", GetType().Name);
        }

        public override void Handle(InternalCommandScheduled @event)
        {
            InvokeHandler((dynamic)@event.Command);
            @event.CompleteEvent();
        }

        private void InvokeHandler<T>(T cmd) where T : INodeCommand
        {
            var nodeHandler = _node as IHandle<T>;

            if (nodeHandler != null)
            {
                _node.FireAtStateMachine<T>();
                nodeHandler.Handle(cmd);
            }
            else
                _logger.Warning(
                    "Received '{nodeCommand}' to be executed, " +
                    "however Node does not have a handler declared for this command",
                    cmd.GetType().AssemblyQualifiedName);
        }
    }
}