using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Raft.Infrastructure.Journaler;
using Raft.Server.Configuration;
using Raft.Server.Handlers;
using Raft.Server.LightInject;

namespace Raft.Server
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<LogRegister>(new PerContainerLifetime());

            serviceRegistry.Register<NodeStateValidator>();
            serviceRegistry.Register<LogEncoder>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<LogWriter>();
            serviceRegistry.Register<CommandFinalizer>();

            serviceRegistry.Register(factory => new JournalerFactory()
                .CreateJournaler(factory.GetInstance<IRaftConfiguration>().JournalConfiguration));

            // TODO: Create binding for IRaftConfiguration...

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
                .Then(factory.GetInstance<LogReplicator>())
                .Then(factory.GetInstance<LogWriter>())
                .Then(factory.GetInstance<CommandFinalizer>());

            return disruptor.Start();
        }
    }
}
