using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using ProtoBuf;
using Raft.Contracts.Persistance;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Infrastructure.Disruptor;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Server.Handlers.Follower;
using Raft.Tests.Unit.TestData.Commands;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers.Follower
{
    [TestFixture]
    public class RpcLogWriterTests
    {
        [Test]
        public void WritesEntriesReceivedFromRpc()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(new NodeProperties());

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();
            var @event = new AppendEntriesRequested
            {
                Entries = new[] {
                    GetSerializedEntry(1L),
                    GetSerializedEntry(2L),
                    GetSerializedEntry(3L)
                }
            };

            Expression<Predicate<DataBlock[]>> match = x => x
                .Select(db => db.Data)
                .SequenceEqual(@event.Entries);

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            writeDataBlocks.Received().WriteBlocks(Arg.Is(match));
        }

        [Test]
        public void SetsMetadataOnBlocksWritten()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(new NodeProperties());

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var @event = new AppendEntriesRequested
            {
                Entries = new[] {
                    GetSerializedEntry(1L),
                    GetSerializedEntry(2L),
                    GetSerializedEntry(3L)
                }
            };

            Expression<Predicate<DataBlock[]>> match = x => x
                .All(b =>
                    b.Metadata.ContainsKey("BodyType") &&
                    b.Metadata["BodyType"].Equals(typeof(LogEntry).AssemblyQualifiedName));

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            writeDataBlocks.Received().WriteBlocks(Arg.Is(match));
        }

        [Test]
        public void DoesNotWriteBlockWhenEntriesIsNull()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var @event = new AppendEntriesRequested
            {
                Entries = null
            };

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            writeDataBlocks.DidNotReceive().WriteBlocks(Arg.Any<DataBlock[]>());
        }

        [Test]
        public void DoesNotWriteBlockWhenEntriesIsEmpty()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var @event = new AppendEntriesRequested
            {
                Entries = new byte[][]{}
            };

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            writeDataBlocks.DidNotReceive().WriteBlocks(Arg.Any<DataBlock[]>());
        }

        [Test]
        public void DeserializesEntriesAndPlacesThemOnEventInLogOrder()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(new NodeProperties { CommitIndex = 5L });

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var @event = new AppendEntriesRequested
            {
                Entries = new []
                {
                    GetSerializedEntry(6L),
                    GetSerializedEntry(7L),
                    GetSerializedEntry(8L)
                },
                EntriesDeserialized = null
            };

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            @event.EntriesDeserialized.Should().NotBeNull();
            @event.EntriesDeserialized.Length.Should().Be(3);
            @event.EntriesDeserialized[0].Index.Should().Be(6L);
            @event.EntriesDeserialized[1].Index.Should().Be(7L);
            @event.EntriesDeserialized[2].Index.Should().Be(8L);
        }

        [TestCase(6L)]
        [TestCase(7L)]
        [TestCase(8L, ExpectedException = typeof(AssertionException))]
        [TestCase(9L)]
        public void ThrowsIfLowestLogIdxIsNotOneGreaterThanCommitIdx(long lowestIdx)
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(new NodeProperties {CommitIndex = 7L});

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = Substitute.For<IPublishToBuffer<InternalCommandScheduled>>();

            var @event = new AppendEntriesRequested
            {
                Entries = new[]
                {
                    GetSerializedEntry(lowestIdx),
                    GetSerializedEntry(10L),
                    GetSerializedEntry(11L)
                },
                EntriesDeserialized = null
            };

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            var actAction = new Action(() => handler.OnNext(@event, 0L, true));

            // Assert
            actAction.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void FiresCommitEntryCommandForEachNewEntry()
        {
            // Arrange
            var raftNode = Substitute.For<INode>();
            raftNode.Properties.Returns(new NodeProperties());

            var nodePublisher = new TestBufferPublisher<InternalCommandScheduled>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();

            var @event = new AppendEntriesRequested
            {
                Entries = new[]
                {
                    GetSerializedEntry(1L),
                    GetSerializedEntry(2L),
                    GetSerializedEntry(3L)
                },
                EntriesDeserialized = null
            };

            var handler = new RpcLogWriter(writeDataBlocks, raftNode, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, true);

            // Assert
            nodePublisher.Events.Count.Should().Be(3);

            var startIdx = 0;
            nodePublisher.Events.ToList().ForEach(ev =>
            {
                ev.Command.Should().BeOfType<CommitEntry>();

                var cmd = ((CommitEntry)ev.Command);
                cmd.Entry
                    .SequenceEqual(@event.Entries[startIdx])
                    .Should().BeTrue();

                cmd.EntryIdx.Should().Be(++startIdx);
            });
        }

        private static byte[] GetSerializedEntry(long logIdx)
        {
            var logEntry = new LogEntry
            {
                Index = logIdx,
                Term = 0L,
                Command = new TestCommand(),
                CommandType = typeof(TestCommand).AssemblyQualifiedName
            };

            using (var ms = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(ms, logEntry, PrefixStyle.Base128);
                return ms.ToArray();
            }
        }
    }
}
