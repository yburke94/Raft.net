using Raft.Configuration;
using Raft.Contracts.Persistance;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Wcf;
using Raft.Service.Contracts;
using Serilog;

namespace Raft.Core.Cluster
{
    internal interface IPeerActorFactory
    {
        PeerActor Create(PeerInfo peerInfo);
    }

    internal class PeerActorFactory : IPeerActorFactory
    {
        private readonly ILogger _logger;
        private readonly INode _node;
        private readonly IRaftConfiguration _configuration;

        public PeerActorFactory(ILogger logger, INode node, IRaftConfiguration configuration)
        {
            _logger = logger;
            _node = node;
            _configuration = configuration;
        }

        public PeerActor Create(PeerInfo peerInfo)
        {
            var proxyFactory = new ServiceProxyFactory<IRaftService>(
                peerInfo.Address, _configuration.RaftServiceBinding, _logger);

            return new PeerActor(peerInfo.NodeId, _node, proxyFactory, _logger);
        }
    }
}