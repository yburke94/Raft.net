using System;
using System.Collections.Generic;
using System.Linq;

namespace Raft.Server
{
    public class LogRegister
    {
        private readonly Dictionary<Guid, byte[]> _logs = new Dictionary<Guid, byte[]>();

        public void AddEncodedLog(Guid eventId, byte[] encodedLog)
        {
            if(_logs.ContainsKey(eventId))
                throw new InvalidOperationException("An encoded log already exisst for this event.");

            _logs.Add(eventId, encodedLog);
        }

        public bool HasLogEntry(Guid eventId)
        {
            return _logs.ContainsKey(eventId);
        }

        public byte[] GetEncodedLog(Guid eventId)
        {
            return _logs[eventId];
        }

        public void EvictEntry(Guid eventId)
        {
            _logs.Remove(eventId);
        }
    }
}
