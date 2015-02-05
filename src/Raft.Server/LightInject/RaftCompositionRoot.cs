using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Raft.Infrastructure.Journaler;
using Raft.Server.Configuration;
using Raft.Server.Handlers;
using Raft.Server.Log;

namespace Raft.Server.LightInject
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.RegisterInstance(new LogEntryRegister(2));

            serviceRegistry.Register<RaftServerContext>(new PerContainerLifetime());

            serviceRegistry.Register<NodeStateValidator>();
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<CommandApplier>();
            serviceRegistry.Register<FaultedCommandHandler>();

            serviceRegistry.Register(factory => new JournalFactory()
                .CreateJournaler(factory.GetInstance<IRaftConfiguration>().JournalConfiguration));

            // TODO: Create binding for IRaftConfiguration...
            // TODO: Make Buffer size configurable...
            serviceRegistry.Register(factory => CreateCommandBuffer(factory, 1024),
                new PerContainerLifetime());
        }

        private static RingBuffer<CommandScheduledEvent> CreateCommandBuffer(IServiceFactory factory, int bufferSize)
        {
            var disruptor = new Disruptor<CommandScheduledEvent>(
                () => new CommandScheduledEvent(),
                new MultiThreadedClaimStrategy(bufferSize),
                new YieldingWaitStrategy(), TaskScheduler.Default);

            disruptor
                .HandleEventsWith(factory.GetInstance<NodeStateValidator>())
                .Then(factory.GetInstance<LogEncoder>())
                .Then(factory.GetInstance<LogWriter>())
                .Then(factory.GetInstance<LogReplicator>())
                .Then(factory.GetInstance<CommandApplier>())
                .Then(factory.GetInstance<FaultedCommandHandler>());

            return disruptor.Start();
        }
    }
}
