using System;

namespace Raft.Core.Cluster
{
    internal class ReplicateRequest
    {
        public byte[] Entry { get; private set; }
        public Action SuccessAction { get; private set; }

        public ReplicateRequest(byte[] entry, Action successAction)
        {
            Entry = entry;
            SuccessAction = successAction;
        }

        public ReplicateRequest Clone()
        {
            var tempSuccessAction = SuccessAction;
            return new ReplicateRequest((byte[])Entry.Clone(), tempSuccessAction);
        }
    }
}