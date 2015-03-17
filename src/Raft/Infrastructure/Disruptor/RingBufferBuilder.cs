using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;

namespace Raft.Infrastructure.Disruptor
{
    internal class RingBufferBuilder<TEvent> where TEvent : class
    {
        private int _bufferSize;

        private Func<TEvent> _eventFactory;
        private Func<int, IClaimStrategy> _claimStrategyFactory;

        private IWaitStrategy _waitStrategy;
        private readonly IList<IEventHandler<TEvent>> _eventHandlers = new List<IEventHandler<TEvent>>();

        public RingBufferBuilder<TEvent> UseBufferSize(int bufferSize)
        {
            if (_bufferSize != 0)
                throw new InvalidOperationException("BufferSize has already been set.");

            if (bufferSize == 0 || (bufferSize & (bufferSize-1)) != 0)
                throw new InvalidOperationException("BufferSize must be a power of 2.");

            _bufferSize = bufferSize;
            return this;
        }

        public RingBufferBuilder<TEvent> UseEventFactory(Func<TEvent> eventFactory)
        {
            if (eventFactory == null)
                throw new ArgumentException("Must supply function to instansiate event.", "eventFactory");

            if (_eventFactory != null)
                throw new InvalidOperationException("Event Factory has already been set.");

            _eventFactory = eventFactory;
            return this;
        }

        public RingBufferBuilder<TEvent> UseDefaultEventCtor()
        {
            if (_eventFactory != null)
                throw new InvalidOperationException("Event Factory has already been set.");

            _eventFactory = () => default(TEvent);
            return this;
        }

        public RingBufferBuilder<TEvent> UseMultipleProducers(bool lowContention)
        {
            if (_claimStrategyFactory != null)
                throw new InvalidOperationException("Producer claim strategy has already been set.");

            _claimStrategyFactory = lowContention
                ? size => new MultiThreadedLowContentionClaimStrategy(size)
                : new Func<int, IClaimStrategy>(size => new MultiThreadedClaimStrategy(size));
            return this;
        }

        public RingBufferBuilder<TEvent> UseSingleProducer()
        {
            if (_claimStrategyFactory != null)
                throw new InvalidOperationException("Producer claim strategy has already been set.");

            _claimStrategyFactory = size => new SingleThreadedClaimStrategy(size);
            return this;
        }

        /// <summary>
        /// Uses a busy spin strategy when waiting for a sequence to become available.
        /// </summary>
        public RingBufferBuilder<TEvent> UseSpinWaitStrategy()
        {
            if (_waitStrategy != null)
                throw new InvalidOperationException("Wait strategy has already been set.");

            _waitStrategy = new BusySpinWaitStrategy();
            return this;
        }

        /// <summary>
        /// Uses a blocking wait strategy that relies on kernel locks when waiting for a sequence to become available.
        /// </summary>
        public RingBufferBuilder<TEvent> UseBlockingWaitStrategy()
        {
            if (_waitStrategy != null)
                throw new InvalidOperationException("Wait strategy has already been set.");

            _waitStrategy = new BlockingWaitStrategy();
            return this;
        }

        /// <summary>
        /// Uses a strategy that will spin for a set number of iterations before yielding control to the CPU when waiting for a sequence to become available.
        /// </summary>
        public RingBufferBuilder<TEvent> UseSpinAndYieldWaitStrategy()
        {
            if (_waitStrategy != null)
                throw new InvalidOperationException("Wait strategy has already been set.");

            _waitStrategy = new YieldingWaitStrategy();
            return this;
        }

        public RingBufferBuilder<TEvent> AddEventHandler(IEventHandler<TEvent> eventHandler)
        {
            if (eventHandler == null)
                throw new ArgumentException("EventHandler must not be null.", "eventHandler");

            _eventHandlers.Add(eventHandler);
            return this;
        }

        public RingBuffer<TEvent> Build()
        {
            if (_bufferSize == 0)
                throw new InvalidOperationException("Buffer size must be set.");

            if (_eventFactory == null)
                throw new InvalidOperationException("Event factory must be specified.");

            if (_claimStrategyFactory == null)
                throw new InvalidOperationException("Claim strategy must be specified.");

            if (_waitStrategy == null)
                throw new InvalidOperationException("Wait strategy must be specified.");

            if (!_eventHandlers.Any())
                throw new InvalidOperationException("At least one event handler must be specified.");

            var disruptor = new Disruptor<TEvent>(
                _eventFactory,
                _claimStrategyFactory(_bufferSize),
                _waitStrategy,
                TaskScheduler.Default);

            EventHandlerGroup<TEvent> handlerGroup = null;

            foreach (var handler in _eventHandlers)
            {
                if (handlerGroup == null)
                    handlerGroup = disruptor.HandleEventsWith(handler);
                else
                    handlerGroup.Then(handler);
            }

            return disruptor.Start();
        }
    }
}
