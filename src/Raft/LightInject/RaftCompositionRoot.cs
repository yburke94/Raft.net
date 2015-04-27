using Raft.Configuration;
using Raft.Contracts;
using Raft.Core.Cluster;
using Raft.Core.Events;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Server.Handlers.Core;
using Raft.Server.Handlers.Follower;
using Raft.Server.Handlers.Leader;

namespace Raft.LightInject
{
    internal class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            // Infrastructure
            serviceRegistry.Register<IEventDispatcher, LightInjectEventDispatcher>();

            // TODO: make this configurable!!!!!!
            serviceRegistry.Register(x => new JournalFactory()
                .CreateJournaler(x.GetInstance<IRaftConfiguration>().JournalConfiguration));

            serviceRegistry.Register<CommandRegister>(new PerContainerLifetime());

            serviceRegistry.Register<IPeerActorFactory, PeerActorFactory>(new PerRequestLifeTime());

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
            // TODO: Configure Serilog
            // TODO: Make Buffer size configurable...

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

            serviceRegistry.Register<IPublishToBuffer<CommandScheduled, CommandExecutionResult>,
                PublishToBuffer<CommandScheduled, CommandExecutionResult>>();

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
            serviceRegistry.Register(x => new RingBufferBuilder<NodeCommandScheduled>()
                .UseBufferSize(2 << 5) // 64
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(new LoggingHandler<NodeCommandScheduled>(x.GetInstance<NodeCommandExecutor>()))
                .Build());

            serviceRegistry.Register<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>,
                PublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
        }
    }
}
