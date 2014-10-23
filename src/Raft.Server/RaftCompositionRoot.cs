using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Raft.Server.Handlers;
using Raft.Server.LightInject;

namespace Raft.Server
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<NodeStateValidator>();
            serviceRegistry.Register<CommandEncoder>();
            serviceRegistry.Register<LogReplicator>();
            serviceRegistry.Register<LogPersistor>();

            serviceRegistry.Register(factory => CreateCommandBuffer(factory, 1024), new PerContainerLifetime());
        }

        private static RingBuffer<CommandScheduledEvent> CreateCommandBuffer(IServiceFactory factory, int bufferSize)
        {
            var disruptor = new Disruptor<CommandScheduledEvent>(
                () => new CommandScheduledEvent(),
                new MultiThreadedClaimStrategy(bufferSize),
                new YieldingWaitStrategy(), TaskScheduler.Default);

            disruptor
                .HandleEventsWith(factory.GetInstance<NodeStateValidator>())
                .Then(factory.GetInstance<CommandEncoder>())
                .Then(factory.GetInstance<LogReplicator>())
                .Then(factory.GetInstance<LogPersistor>());

            return disruptor.Start();
        }
    }
}
