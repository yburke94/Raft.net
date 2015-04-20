using FluentAssertions;
using NUnit.Framework;
using Raft.Service;

namespace Raft.Tests.Unit.Service
{
    [TestFixture]
    public class PeerServiceTests
    {
        public void GetPeersInClusterReturnsAllPeerNodes()
        {
            // Arrange
            var service = new PeerService();

            // Act
            var results = service.GetPeersInCluster();

            // Assert
            results.Count.Should().Be(3);
        }

    }
}
