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
                Log.Debug("Handler '{handler}' successfully handled event '{eventType}'",
                    _handler.GetType(), data.GetType());
            }
            catch (Exception exc)
            {
                Log.Error(exc, "An error occurred handling event '{eventType}' with handler '{handler}'.",
                    data.GetType(), _handler.GetType());
            }
        }
    }
}
