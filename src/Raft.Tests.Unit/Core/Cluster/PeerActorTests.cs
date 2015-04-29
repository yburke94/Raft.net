using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NUnit.Framework;
using Raft.Contracts.Persistance;
using Raft.Core.Cluster;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Data;
using Raft.Infrastructure.Wcf;
using Raft.Service.Contracts;
using Serilog;

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var logger = Substitute.For<ILogger>();

            // Act
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse {Success = true});

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse { Success = true });

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(new AppendEntriesResponse {Success = true});

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);
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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            var appendEntriesCalls = 0;
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

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var raftService = Substitute.For<IRaftService>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var raftService = Substitute.For<IRaftService>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();
            var raftService = Substitute.For<IRaftService>();

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

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

        [Test]
        public void SendsAllEntriesFromNextIndexToCurrentRequestEntryIndex()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CommitIndex = 4L,
                CurrentTerm = 3L,
            });
            node.Log.Returns(CreateLog(4L));

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();

            var oldBlockData = BitConverter.GetBytes(100);
            var getDataBlocks = Substitute.For<IGetDataBlocks>();
            getDataBlocks.GetBlock(Arg.Any<DataRequest>())
                .Returns(new DataBlock {Data = oldBlockData});

            var raftService = Substitute.For<IRaftService>();

            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

            var fails = -1;
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x => new AppendEntriesResponse
                {
                    // Return success when NextIndex will equal 1
                    Success = (++fails) == 4
                });

            var request = new ReplicateRequest(5L, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            getDataBlocks.Received(fails).GetBlock(Arg.Any<DataRequest>());

            Expression<Predicate<AppendEntriesRequest>> match = req =>
                req.Entries.Count().Equals(fails + 1) &&
                req.Entries.Last().SequenceEqual(request.Entry) &&
                req.Entries.First().SequenceEqual(oldBlockData);

            raftService.Received().AppendEntries(Arg.Is(match));
        }

        [Test]
        public void LogsErrorIfServiceThrowsAnException()
        {
            // Arrange
            var node = Substitute.For<INode>();
            node.Properties.Returns(new NodeProperties
            {
                CommitIndex = 4L,
                CurrentTerm = 3L,
            });
            node.Log.Returns(CreateLog(4L));

            var serviceFactory = Substitute.For<IServiceProxyFactory<IRaftService>>();

            var getDataBlocks = Substitute.For<IGetDataBlocks>();

            var raftService = Substitute.For<IRaftService>();
            serviceFactory.GetProxy().Returns(raftService);

            var logger = Substitute.For<ILogger>();
            logger.ForContext(Arg.Any<string>(), Arg.Any<object>()).Returns(logger);

            var peerActor = new PeerActor(Guid.NewGuid(), node, serviceFactory, getDataBlocks, logger);

            var fails = 0;
            raftService.AppendEntries(Arg.Any<AppendEntriesRequest>())
                .Returns(x =>
                {
                    if (fails++ == 1)
                    {
                        return new AppendEntriesResponse
                        {
                            Success = true
                        };
                    }

                    throw new Exception();
                });

            var request = new ReplicateRequest(5L, new byte[6], () => { });

            // Act
            peerActor.Handle(request);

            // Assert
            logger.Received().Error(Arg.Any<Exception>(), Arg.Any<string>(), Arg.Any<object[]>());
        }

        private static InMemoryLog CreateLog(long count)
        {
            var log = new InMemoryLog();
            for (var i = 1; i <= count; i++)
                log.SetLogEntry(i, 1L);

            return log;
        }
    }
}
