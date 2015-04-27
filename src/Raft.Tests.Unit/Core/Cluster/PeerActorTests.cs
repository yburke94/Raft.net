using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core.Cluster;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Data;
using Raft.Infrastructure.Wcf;
using Raft.Service.Contracts;

namespace Raft.Tests.Unit.Core.Cluster
{
    [TestFixture]
    public class PeerActorTests
    {
        [Test]
        public void NextIndexInitializedToLeaderCommitPlus1()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties{CommitIndex = 3L});

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();

            // Act
            var peerActor = new PeerActor(node, serviceFactory);

            // Assert
            peerActor.NextIndex.Should().Be(node.Properties.CommitIndex+1);
        }

        [Test]
        public void CallsAppendEntriesWithLatestEntryOnInitialRequest()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties());

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var request = new ReplicateRequest(new byte[6], () => { });

            Expression<Predicate<AppendEntriesRequest>> match = x =>
                x.Entries.Count() == 1 &&
                x.Entries[0].SequenceEqual(request.Entry);

            // Act
            peerActor.Handle(request);

            // Assert
            raftService.Received(1).AppendEntries(Arg.Is(match));
        }

        [Test]
        public void CallsAppendEntriesWithAllLeaderNodePropertiesSet()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 20L
            });

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var request = new ReplicateRequest(new byte[6], () => { });

            Expression<Predicate<AppendEntriesRequest>> match = x =>
                x.LeaderId == node.Properties.NodeId &&
                x.Term == node.Properties.CurrentTerm &&
                x.PreviousLogIndex == node.Properties.CommitIndex &&
                x.PreviousLogTerm == node.Properties.CurrentTerm &&
                x.LeaderCommit == node.Properties.CommitIndex;

            // Act
            peerActor.Handle(request);

            // Assert
            raftService.Received(1).AppendEntries(Arg.Is(match));
        }

        [Test]
        public void SuccessfulAppendEntriesWillIncrementNextIndex()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 20L
            });

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse {Success = true});

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);
            peerActor.NextIndex.Should().Be(node.Properties.CommitIndex + 1);

            var request = new ReplicateRequest(new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            peerActor.NextIndex.Should().Be(node.Properties.CommitIndex + 2);
        }
    }
}
