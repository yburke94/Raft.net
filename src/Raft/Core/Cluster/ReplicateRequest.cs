using System;

namespace Raft.Core.Cluster
{
    internal class ReplicateRequest
    {
        public long EntryIdx { get; private set; }
        public byte[] Entry { get; private set; }
        public Action SuccessAction { get; private set; }

        public ReplicateRequest(long entryIdx, byte[] entry, Action successAction)
        {
            EntryIdx = entryIdx;
            Entry = entry;
            SuccessAction = successAction;
        }

        public ReplicateRequest Clone()
        {
            var tempSuccessAction = SuccessAction;
            return new ReplicateRequest(EntryIdx, (byte[])Entry.Clone(), tempSuccessAction);
        }
    }
}