using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server.Configuration;
using Raft.Server.Events;
using Raft.Server.Handlers.Follower;
using Raft.Server.Handlers.Leader;

namespace Raft.Server.LightInject
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            // Leader event handlers
            serviceRegistry.Register<NodeStateValidator>();
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<CommandFinalizer>();

            // Follower event handlers
            serviceRegistry.Register<RpcCommandApplier>();
            serviceRegistry.Register<RpcLogWriter>();

            serviceRegistry.Register(factory => new JournalFactory()
                .CreateJournaler(factory.GetInstance<IRaftConfiguration>().JournalConfiguration));

            // TODO: Create binding for IRaftConfiguration...
            // TODO: Make Buffer size configurable...

            // TODO: Bind IServiceLocator to the locator passed in by config!

            // Create Leader ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommandScheduled>()
                .UseBufferSize(2<<7) // 256
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<NodeStateValidator>())
                .AddEventHandler(x.GetInstance<LogEncoder>())
                .AddEventHandler(x.GetInstance<LogWriter>())
                .AddEventHandler(x.GetInstance<LogReplicator>())
                .AddEventHandler(x.GetInstance<CommandFinalizer>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<CommandScheduled>,
                PublishToBuffer<CommandScheduled>>();

            // Create Follower commit ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommitCommandRequested>()
                .UseBufferSize(2<<6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<RpcLogWriter>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<CommitCommandRequested>,
                PublishToBuffer<CommitCommandRequested>>();

            // Create Follower apply ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<ApplyCommandRequested>()
                .UseBufferSize(2 << 6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<RpcCommandApplier>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<ApplyCommandRequested>,
                PublishToBuffer<ApplyCommandRequested>>();
        }
    }
}
