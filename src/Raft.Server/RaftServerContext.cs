using System;
using System.Collections.Generic;

namespace Raft.Server
{
    public sealed class RaftServerContext
    {
        private readonly Dictionary<string, object> _contextobjects = new Dictionary<string, object>();

        public T GetFromContext<T>(string key) where T : class
        {
            if (!_contextobjects.ContainsKey(key))
                throw new KeyNotFoundException("The RaftContext does not contain an entry for the specified key: " + key);

            var val = _contextobjects[key];
            var typedVal = val as T;
            
            if (typedVal == null)
                throw new InvalidCastException(string.Format("Failed to cast object of type: {0} to {1}", val.GetType(), typeof(T).Name));

            return typedVal;
        }

        public void Add(string key, object value)
        {
            _contextobjects.Add(key, value);
        }
    }
}