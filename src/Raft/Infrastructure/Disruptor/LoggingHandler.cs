using System;
using Disruptor;
using Serilog;

namespace Raft.Infrastructure.Disruptor
{
    public class LoggingHandler<TEvent> : IEventHandler<TEvent>
    {
        private readonly IEventHandler<TEvent> _handler;

        public LoggingHandler(IEventHandler<TEvent> handler)
        {
            _handler = handler;
        }

        public void OnNext(TEvent data, long sequence, bool endOfBatch)
        {
            try
            {
                _handler.OnNext(data, sequence, endOfBatch);
                Log.ForContext("BufferHandler", _handler.GetType().AssemblyQualifiedName)
                    .Debug("Handler successfully handled event '{eventType}'",
                    data.GetType().AssemblyQualifiedName);
            }
            catch (Exception exc)
            {
                Log.ForContext("BufferHandler", _handler.GetType().AssemblyQualifiedName)
                    .Error(exc, "An error occurred handling event '{eventType}'.",
                    data.GetType().AssemblyQualifiedName);
            }
        }
    }
}
