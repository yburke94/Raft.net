using System.ServiceModel;
using System.ServiceModel.Channels;
using Serilog;

namespace Raft.Infrastructure.Wcf
{
    internal class ServiceProxyFactory<TService> : IServiceProxyFactory<TService>
    {
        private readonly ILogger _logger;
        private readonly ChannelFactory<TService> _serviceChannelFactory;

        public EndpointAddress Address { get; private set; }
        public Binding Binding { get; private set; }

        public ServiceProxyFactory(string endpointAddress, Binding endpointBinding, ILogger logger)
        {
            Address = new EndpointAddress(endpointAddress);
            Binding = endpointBinding;
            _logger = logger
                .ForContext("Service", typeof (TService).Name)
                .ForContext("Address", endpointAddress);

            _serviceChannelFactory = new ChannelFactory<TService>(
                Binding, Address);
        }

        public TService GetProxy()
        {
            return (TService)new ServiceClientProxy<TService>(
                _serviceChannelFactory.CreateChannel(), _logger)
                .GetTransparentProxy();
        }
    }
}