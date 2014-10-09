using System.Linq;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Raft.Server.LightInject;

namespace Raft.Server
{
    class RaftCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry serviceRegistry)
        {
            serviceRegistry.Register<IEventHandler<CommandScheduledEvent>, ServerStateValidator>(new PerContainerLifetime());
            serviceRegistry.Register(factory => CreateRingBuffer<CommandScheduledEvent>(factory, 1024), new PerContainerLifetime());
        }

        private static RingBuffer<T> CreateRingBuffer<T>(IServiceFactory serviceFactory, int bufferSize) where T : class, new()
        {
            var disruptor = new Disruptor<T>(() => new T(), new MultiThreadedClaimStrategy(bufferSize),
                new YieldingWaitStrategy(), TaskScheduler.Default);

            disruptor.HandleEventsWith(serviceFactory.GetAllInstances<IEventHandler<T>>().ToArray());
            return disruptor.Start();
        }
    }
}
