using System.IO;
using Disruptor;
using ProtoBuf;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Events;
using Raft.Server.Log;

namespace Raft.Server.Handlers.Follower
{
    internal class RpcLogWriter : IEventHandler<CommitCommandRequested>
    {
        private readonly IJournal _journal;
        private readonly IRaftNode _raftNode;

        public RpcLogWriter(IJournal journal, IRaftNode raftNode)
        {
            _journal = journal;
            _raftNode = raftNode;
        }

        public void OnNext(CommitCommandRequested data, long sequence, bool endOfBatch)
        {
            LogEntry entry = null;

            try
            {
                using (var stream = new MemoryStream(data.Entry, false))
                {
                    entry = Serializer.DeserializeWithLengthPrefix<LogEntry>(stream, PrefixStyle.Base128);
                }

                // TODO: Generate checksum and compare?.
                
                _journal.WriteBlock(data.Entry);
                _raftNode.CommitLogEntry(entry.Index); // TODO: ADD Term....
            }
            catch
            {
                //  TODO: Add logging and log error
            }
        }
    }
}
