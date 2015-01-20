using System.Collections.Generic;
using Raft.Server.Log;
using Raft.Server.Messages.AppendEntries;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 3 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogReplicator*
    ///     LogWriter
    ///     CommandFinalizer
    /// </summary>
    internal class LogReplicator : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IList<PeerNode> _peers;
        private readonly LogRegister _logRegister;

        public LogReplicator(IList<PeerNode> peers, LogRegister logRegister)
        {
            _peers = peers;
            _logRegister = logRegister;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var bytes = _logRegister.GetEncodedLog(@event.Id);

            var request = new AppendEntriesRequest {
                Entries = new[] {
                    bytes
                }
            };

            foreach (var peerNode in _peers)
            {
                peerNode.Channel.AppendEntries(request);
            }
        }

    }
}
