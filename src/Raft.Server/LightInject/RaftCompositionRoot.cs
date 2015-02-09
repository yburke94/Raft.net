using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server.Configuration;
using Raft.Server.Handlers.Follower;
using Raft.Server.Handlers.Leader;
using Raft.Server.Log;
using Raft.Server.Services;

namespace Raft.Server.LightInject
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.RegisterInstance(new EncodedEntryRegister());

            serviceRegistry.Register<RaftServerContext>(new PerContainerLifetime());

            // Leader event handlers
            serviceRegistry.Register<NodeStateValidator>();
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<CommandApplier>();

            // Follower event handlers
            serviceRegistry.Register<RpcCommandApplier>();
            serviceRegistry.Register<RpcLogWriter>();

            serviceRegistry.Register(factory => new JournalFactory()
                .CreateJournaler(factory.GetInstance<IRaftConfiguration>().JournalConfiguration));

            // TODO: Create binding for IRaftConfiguration...
            // TODO: Make Buffer size configurable...

            // Create Leader ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommandScheduledEvent>()
                .UseBufferSize(2<<7) // 256
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<NodeStateValidator>())
                .AddEventHandler(x.GetInstance<LogEncoder>())
                .AddEventHandler(x.GetInstance<LogReplicator>())
                .AddEventHandler(x.GetInstance<LogWriter>())
                .AddEventHandler(x.GetInstance<CommandApplier>())
                .Build());

            serviceRegistry.Register<IEventPublisher<CommandScheduledEvent>,
                DisruptorEventPublisher<CommandScheduledEvent>>();

            // Create Follower commit ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommitRequestedEvent>()
                .UseBufferSize(2<<6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<RpcLogWriter>())
                .Build());

            serviceRegistry.Register<IEventPublisher<CommitRequestedEvent>,
                DisruptorEventPublisher<CommitRequestedEvent>>();

            // Create Follower apply ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<ApplyRequestedEvent>()
                .UseBufferSize(2 << 6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<RpcCommandApplier>())
                .Build());

            serviceRegistry.Register<IEventPublisher<ApplyRequestedEvent>,
                DisruptorEventPublisher<ApplyRequestedEvent>>();
        }
    }
}
