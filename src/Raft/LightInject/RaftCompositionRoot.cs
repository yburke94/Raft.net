using Raft.Configuration;
using Raft.Core.Cluster;
using Raft.Core.Events;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Compression;
using Raft.Infrastructure.Disruptor;
using Raft.Server;
using Raft.Server.BufferEvents;
using Raft.Server.Handlers.Core;
using Raft.Server.Handlers.Follower;
using Raft.Server.Handlers.Leader;

namespace Raft.LightInject
{
    internal class RaftIoc
    {
        internal static ServiceContainer Container = new ServiceContainer();

        public RaftIoc()
        {
            // Disable automatic propery injection.
            Container.EnableAnnotatedPropertyInjection();
            Container.RegisterFrom<RaftCompositionRoot>();
        }
    }

    internal class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            // Infrastructure
            serviceRegistry.Register<IEventDispatcher, LightInjectEventDispatcher>();

            serviceRegistry.Register(x => x.GetInstance<IRaftConfiguration>().GetBlockWriter());
            serviceRegistry.Register(x => x.GetInstance<IRaftConfiguration>().GetBlockReader());

            serviceRegistry.Register<CommandRegister>(new PerContainerLifetime());

            serviceRegistry.Register<IPeerActorFactory, PeerActorFactory>(new PerRequestLifeTime());

            serviceRegistry.Register<ICompressBlock, SnappyCompression>();
            serviceRegistry.Register<IDecompressBlock, SnappyCompression>();

            // State machine
            serviceRegistry.Register<Node>(new PerContainerLifetime());
            serviceRegistry.Register<INode>(x => x.GetInstance<Node>());

            // Event handlers
            serviceRegistry.Register<IHandle<TermChanged>>(x => x.GetInstance<CommandRegister>());

            // Leader buffer event handlers
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<CommandFinalizer>();

            // Follower buffer event handlers
            serviceRegistry.Register<RpcCommandApplier>();
            serviceRegistry.Register<RpcLogWriter>();

            // Core buffer event handlers
            serviceRegistry.Register<NodeCommandExecutor>();

            // TODO: Create binding for IRaftConfiguration...

            serviceRegistry.Register(x => 
                x.GetInstance<IRaftConfiguration>().GetLogger()
                .ForContext("Framework", "Raft.Net", false));

            // TODO: Make Buffer size configurable... But difficult to do so.

            // TODO: Bind IServiceLocator to the locator passed in by config!

            // Create Leader ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommandScheduled>()
                .UseBufferSize(2<<7) // 256
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(new LoggingHandler<CommandScheduled>(x.GetInstance<LogEncoder>()))
                .AddEventHandler(new LoggingHandler<CommandScheduled>(x.GetInstance<LogWriter>()))
                .AddEventHandler(new LoggingHandler<CommandScheduled>(x.GetInstance<LogReplicator>()))
                .AddEventHandler(new LoggingHandler<CommandScheduled>(x.GetInstance<CommandFinalizer>()))
                .Build());

            serviceRegistry.Register<IPublishToBuffer<CommandScheduled>,
                PublishToBuffer<CommandScheduled>>();

            // Create Follower commit ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<AppendEntriesRequested>()
                .UseBufferSize(2<<6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(new LoggingHandler<AppendEntriesRequested>(x.GetInstance<RpcLogTruncator>()))
                .AddEventHandler(new LoggingHandler<AppendEntriesRequested>(x.GetInstance<RpcLogWriter>()))
                .AddEventHandler(new LoggingHandler<AppendEntriesRequested>(x.GetInstance<RpcCommandApplier>()))
                .Build());

            serviceRegistry.Register<IPublishToBuffer<AppendEntriesRequested>,
                PublishToBuffer<AppendEntriesRequested>>();

            // Create core ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<InternalCommandScheduled>()
                .UseBufferSize(2 << 5) // 64
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(new LoggingHandler<InternalCommandScheduled>(x.GetInstance<NodeCommandExecutor>()))
                .Build());

            serviceRegistry.Register<IPublishToBuffer<InternalCommandScheduled>,
                PublishToBuffer<InternalCommandScheduled>>();
        }
    }
}
