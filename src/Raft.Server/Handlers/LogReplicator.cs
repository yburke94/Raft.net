using System.Collections.Generic;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;
using Raft.Server.Messages.AppendEntries;

namespace Raft.Server.Handlers
{
    /// <summary>
    /// 3 of 5 EventHandlers for scheduled state machine commands.
    /// Order of execution:
    ///     NodeStateValidator
    ///     LogEncoder
    ///     LogWriter
    ///     LogReplicator*
    ///     CommandApplier
    /// </summary>
    internal class LogReplicator : RaftEventHandler, ISkipInternalCommands
    {
        private readonly IList<PeerNode> _peers;
        private readonly EncodedLogRegister _encodedLogRegister;

        public LogReplicator(IList<PeerNode> peers, EncodedLogRegister encodedLogRegister)
        {
            _peers = peers;
            _encodedLogRegister = encodedLogRegister;
        }

        public override void Handle(CommandScheduledEvent @event)
        {
            var bytes = _encodedLogRegister.GetEncodedLog(@event.Id);

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
