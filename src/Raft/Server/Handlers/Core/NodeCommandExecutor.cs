using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;

namespace Raft.Server.Handlers.Core
{
    // TODO: Test
    internal class NodeCommandExecutor : BufferEventHandler<InternalCommandScheduled>
    {
        private readonly Node _node;

        public NodeCommandExecutor(Node node)
        {
            _node = node;
        }

        public override void Handle(InternalCommandScheduled @event)
        {
            Handle(@event.Command);
            @event.CompleteEvent();
        }

        private void Handle<T>(T cmd) where T : INodeCommand
        {
            _node.FireAtStateMachine<T>();
            var nodeHandler = _node as IHandle<T>;
            if (nodeHandler != null)
                nodeHandler.Handle(cmd);
        }
    }
}