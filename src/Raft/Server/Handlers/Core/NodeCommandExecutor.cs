using System;
using Disruptor;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Server.BufferEvents;
using Raft.Server.Data;

namespace Raft.Server.Handlers.Core
{
    internal class NodeCommandExecutor : IEventHandler<NodeCommandScheduled>
    {
        private readonly Node _node;

        public NodeCommandExecutor(Node node)
        {
            _node = node;
        }

        public void OnNext(NodeCommandScheduled data, long sequence, bool endOfBatch)
        {
            try
            {
                Handle(data.Command);
            }
            catch (Exception e)
            {
                data.CompletionSource.SetException(e);
            }
            finally
            {
                data.CompletionSource.SetResult(new NodeCommandResult(true));
            }
            
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