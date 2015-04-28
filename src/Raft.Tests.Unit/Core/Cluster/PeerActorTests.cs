using System;
using System.Collections.Generic;
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
            node.Log.Returns(CreateLog(1L));

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse {Success = true});

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var request = new ReplicateRequest(1L, new byte[6], () => { });

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
            node.Log.Returns(CreateLog(20L));

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse { Success = true });

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var request = new ReplicateRequest(1L, new byte[6], () => { });

            Expression<Predicate<AppendEntriesRequest>> match = x =>
                x.LeaderId == node.Properties.NodeId &&
                x.Term == node.Properties.CurrentTerm &&
                x.LeaderCommit == node.Properties.CommitIndex;

            // Act
            peerActor.Handle(request);

            // Assert
            raftService.Received(1).AppendEntries(Arg.Is(match));
        }

        [Test]
        public void SuccessfulAppendEntriesWillSetNextIndexToOneMoreThanEntryIndex()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 20L
            });
            node.Log.Returns(CreateLog(20L));

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse {Success = true});

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);
            peerActor.NextIndex.Should().Be(node.Properties.CommitIndex + 1);

            var request = new ReplicateRequest(23L, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            peerActor.NextIndex.Should().Be(request.EntryIdx + 1);
        }

        [TestCase(3)]
        [TestCase(5)]
        [TestCase(7)]
        public void WillTryAgainWhenUnsuccessfulAppendEntriesUntilSuccessful(int failCount)
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 20L
            });
            node.Log.Returns(CreateLog(20L));

            var appendEntriesCalls = 0;

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x =>
                {
                    appendEntriesCalls++;
                    return new AppendEntriesResponse
                    {
                        Success = failCount == appendEntriesCalls
                    };
                });

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var request = new ReplicateRequest(node.Properties.CommitIndex + 1, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            raftService.Received(failCount).AppendEntries(Arg.Any<AppendEntriesRequest>());
        }

        [Test]
        public void WillDecrementNextIndexWithEachUnsuccessfulAttempt()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 20L
            });
            node.Log.Returns(CreateLog(20L));

            var raftService = Substitute.For<IRaftService>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var failCount = 0;
            var nextIndexes = new List<long>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x =>
                {
                    nextIndexes.Add(peerActor.NextIndex);
                    failCount++;
                    return new AppendEntriesResponse
                    {
                        Success = failCount==7
                    };
                });

            var request = new ReplicateRequest(node.Properties.CommitIndex + 1, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            nextIndexes.Count.Should().Be(failCount);
            for (var i = 0; i < failCount; i++)
                nextIndexes[i].Should().Be((node.Properties.CommitIndex + 1) - i);
        }

        [Test]
        public void WillNotDecrementNextIndexWhenEqualTo1()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CurrentTerm = 3L,
                CommitIndex = 2L
            });
            node.Log.Returns(CreateLog(2L));

            var raftService = Substitute.For<IRaftService>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);

            var failCount = 0;
            var lastNextIndexValueBeforeSuccess = 0L;
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x =>
                {
                    failCount++;
                    lastNextIndexValueBeforeSuccess = peerActor.NextIndex;
                    return new AppendEntriesResponse
                    {
                        Success = failCount.Equals((int)node.Properties.CommitIndex + 2)
                    };
                });

            var request = new ReplicateRequest(node.Properties.CommitIndex + 1, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            lastNextIndexValueBeforeSuccess.Should().Be(1);
        }

        [Test]
        public void SetsPreviousLogIndexAndTermToValuesForEntryPreceedingNextIndex()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CommitIndex = 4L,
                CurrentTerm = 3L,
            });
            node.Log.Returns(new InMemoryLog());

            // Initialize log up to commit index
            for (var i = 1; i <= node.Properties.CommitIndex; i++)
                node.Log.SetLogEntry(i, i<3 ? 1L : 2L);

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            serviceFactory.GetProxy().Returns(raftService);

            var peerActor = new PeerActor(node, serviceFactory);
            peerActor.NextIndex.Should().Be(5L); // Next index will be initialized to 5

            var nextIdxAndPrevEntryInfoList = new List<Tuple<long, long, long>>();
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x =>
                {
                    // Each call to AppendEntries saves the NextIndex and the information
                    // passed about the previous entry.
                    nextIdxAndPrevEntryInfoList.Add(Tuple.Create(
                        peerActor.NextIndex,
                        x.Arg<AppendEntriesRequest>().PreviousLogIndex,
                        x.Arg<AppendEntriesRequest>().PreviousLogTerm));

                    return new AppendEntriesResponse
                    {
                        // Return success when NextIndex will equal 1
                        Success = nextIdxAndPrevEntryInfoList.Count.Equals((int)node.Properties.CommitIndex)
                    };
                });

            var request = new ReplicateRequest(5L, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            foreach (var nextIdxAndPrevEntryInfo in nextIdxAndPrevEntryInfoList)
            {
                var nextIndex = nextIdxAndPrevEntryInfo.Item1;
                var prevLogIndex = nextIdxAndPrevEntryInfo.Item2;
                var prevLogTerm = nextIdxAndPrevEntryInfo.Item3;

                // previousLogIndex should be the entry index that preceeds new entries.
                nextIndex.Should().Be(prevLogIndex + 1);

                // previousLogTerm should be the Term for the entry that preceeds new entries.
                prevLogTerm.Should().Be(node.Log[prevLogIndex]);
            }
        }

        private InMemoryLog CreateLog(long count)
        {
            var log = new InMemoryLog();
            for (var i = 1; i <= count; i++)
                log.SetLogEntry(i, 1L);

            return log;
        }
    }
}
