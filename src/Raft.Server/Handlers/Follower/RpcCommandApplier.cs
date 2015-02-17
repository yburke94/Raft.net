using Disruptor;
using Microsoft.Practices.ServiceLocation;
using Raft.Core;
using Raft.Infrastructure.Extensions;
using Raft.Server.Events;
using Raft.Server.Registers;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcCommandApplier : IEventHandler<ApplyCommandRequested>
    {
        private readonly IRaftNode _raftNode;
        private readonly CommandRegister _commandRegister;
        private readonly IServiceLocator _serviceLocator;

        public RpcCommandApplier(IRaftNode raftNode, CommandRegister commandRegister, IServiceLocator serviceLocator)
        {
            _raftNode = raftNode;
            _commandRegister = commandRegister;
            _serviceLocator = serviceLocator;
        }

        public void OnNext(ApplyCommandRequested data, long sequence, bool endOfBatch)
        {
            var appliedDifference = data.LogIdx-_raftNode.LastApplied;
            var logsToApply = EnumerableUtilities.Range(_raftNode.LastApplied + 1, (int)appliedDifference);
            foreach (var logIdx in logsToApply)
            {
                var command = _commandRegister.Get(_raftNode.CurrentTerm, logIdx);

                // The term may have been increased before the command was applied. In which case, rely on log matching to fix.
                if (command == null) continue;

                command.Execute(_serviceLocator);
                _raftNode.ApplyCommand(logIdx);
            }
        }
    }
}
