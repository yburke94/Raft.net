using System.Linq;
using Raft.Infrastructure;

namespace Raft.LightInject
{
    internal class LightInjectEventDispatcher : IEventDispatcher
    {
        private readonly IServiceFactory _factory;

        public LightInjectEventDispatcher(IServiceFactory factory)
        {
            _factory = factory;
        }

        public void Publish<TEvent>(TEvent @event)
        {
            _factory.GetAllInstances<ISubscribe<TEvent>>().ToList()
                .ForEach(x => x.Handle(@event));
        }
    }
}
