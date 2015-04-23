using System;

namespace Raft.Core.Cluster
{
    internal class ReplicateRequest
    {
        private readonly Action _successAction;
        private readonly byte[] _entry;

        public ReplicateRequest(byte[] entry, Action successAction)
        {
            _entry = entry;
            _successAction = successAction;
        }

        public ReplicateRequest Clone()
        {
            var tempSuccessAction = _successAction;
            return new ReplicateRequest((byte[])_entry.Clone(), tempSuccessAction);
        }
    }
}