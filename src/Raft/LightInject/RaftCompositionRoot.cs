using Raft.Configuration;
using Raft.Contracts;
using Raft.Core.StateMachine;
using Raft.Infrastructure;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
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
            serviceRegistry.Register<IWriteDataBlocks>(factory => new JournalFactory()
                .CreateJournaler(factory.GetInstance<IRaftConfiguration>().JournalConfiguration));

            // State machine
            serviceRegistry.Register<Node>(new PerContainerLifetime());
            serviceRegistry.Register<INode>(factory => factory.GetInstance<Node>());

            // Leader event handlers
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<CommandFinalizer>();

            // Follower event handlers
            serviceRegistry.Register<RpcCommandApplier>();
            serviceRegistry.Register<RpcLogWriter>();

            // Core event handlers
            serviceRegistry.Register<NodeCommandExecutor>();

            // TODO: Create binding for IRaftConfiguration...
            // TODO: Make Buffer size configurable...

            // TODO: Bind IServiceLocator to the locator passed in by config!

            // Create Leader ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<CommandScheduled>()
                .UseBufferSize(2<<7) // 256
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<LogEncoder>())
                .AddEventHandler(x.GetInstance<LogWriter>())
                .AddEventHandler(x.GetInstance<LogReplicator>())
                .AddEventHandler(x.GetInstance<CommandFinalizer>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<CommandScheduled, CommandExecutionResult>,
                PublishToBuffer<CommandScheduled, CommandExecutionResult>>();

            // Create Follower commit ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<AppendEntriesRequested>()
                .UseBufferSize(2<<6) // 128
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<RpcLogWriter>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<AppendEntriesRequested>,
                PublishToBuffer<AppendEntriesRequested>>();

            // Create core ring buffer
            serviceRegistry.Register(x => new RingBufferBuilder<NodeCommandScheduled>()
                .UseBufferSize(2 << 5) // 64
                .UseDefaultEventCtor()
                .UseMultipleProducers(false)
                .UseSpinAndYieldWaitStrategy()
                .AddEventHandler(x.GetInstance<NodeCommandExecutor>())
                .Build());

            serviceRegistry.Register<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>,
                PublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
        }
    }
}
