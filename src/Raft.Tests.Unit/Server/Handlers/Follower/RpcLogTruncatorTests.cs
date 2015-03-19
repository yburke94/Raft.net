using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Exceptions;
using NUnit.Framework;
using ProtoBuf;
using Raft.Core.Commands;
using Raft.Core.StateMachine;
using Raft.Core.StateMachine.Data;
using Raft.Infrastructure.Disruptor;
using Raft.Infrastructure.Journaler;
using Raft.Server.BufferEvents;
using Raft.Server.Data;
using Raft.Server.Handlers.Follower;
using Raft.Tests.Unit.TestHelpers;

namespace Raft.Tests.Unit.Server.Handlers.Follower
{
    [TestFixture]
    public class RpcLogTruncatorTests
    {
        [Test]
        public void CreatesAndWritesEntryForTruncateCommand()
        {
            // Arrange
            var @event = new AppendEntriesRequested
            {
                PreviousLogTerm = 1,
                PreviousLogIndex = 5
            };

            var nodeData = new NodeData
            {
                CommitIndex = 20,
                CurrentTerm = 2
            };

            byte[] blockWritten = null;

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            writeDataBlocks
                .When(x => x.WriteBlock(Arg.Any<byte[]>()))
                .Do(x => blockWritten = x.Arg<byte[]>());

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var handler = new RpcLogTruncator(raftNode, writeDataBlocks, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, false);

            // Assert
            writeDataBlocks.Received().WriteBlock(Arg.Any<byte[]>());
            blockWritten.Should().NotBeNull();
            using (var ms = new MemoryStream(blockWritten))
            {
                Serializer.DeserializeWithLengthPrefix<TruncateLogCommandEntry>(ms, PrefixStyle.Base128)
                    .TruncateFromIndex.Should().Be(@event.PreviousLogIndex);
            }
        }

        [Test]
        public void DoesNotWriteEntryForTruncateCommandWhenPrevLogIdxIsNullOrPrevLogTermIsNull()
        {
            // Arrange
            var @event = new AppendEntriesRequested
            {
                PreviousLogIndex = null,
                PreviousLogTerm = null
            };

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var raftNode = Substitute.For<INode>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            
            var handler = new RpcLogTruncator(raftNode, writeDataBlocks, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, false);

            // Assert
            writeDataBlocks.DidNotReceive().WriteBlock(Arg.Any<byte[]>());
        }

        [TestCase(35, ExpectedException = typeof(InvalidOperationException))]
        [TestCase(30)]
        [TestCase(29, ExpectedException = typeof(ReceivedCallsException))]
        public void DoesNotWriteEntryForTruncateCommandWhenPrevIdxIsGreaterThanOrEqualToCommitIdx(long prevIdx)
        {
            // Arrange
            var @event = new AppendEntriesRequested
            {
                PreviousLogTerm = 1,
                PreviousLogIndex = prevIdx
            };

            var nodeData = new NodeData
            {
                CommitIndex = 30,
                CurrentTerm = 2
            };

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var handler = new RpcLogTruncator(raftNode, writeDataBlocks, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, false);

            // Assert
            writeDataBlocks.DidNotReceive().WriteBlock(Arg.Any<byte[]>());
        }

        [Test]
        public void ThrowsWhenPrevTermIsGreaterThanCurrentTerm()
        {
            // Arrange
            var @event = new AppendEntriesRequested
            {
                PreviousLogTerm = 3,
                PreviousLogIndex = 5
            };

            var nodeData = new NodeData
            {
                CommitIndex = 20,
                CurrentTerm = 2
            };

            var nodePublisher = Substitute.For<IPublishToBuffer<NodeCommandScheduled, NodeCommandResult>>();
            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var handler = new RpcLogTruncator(raftNode, writeDataBlocks, nodePublisher);

            // Act
            var actAction = new Action(() => handler.OnNext(@event, 0L, false));

            // Assert
            actAction.ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void PublishesTruncateLogCommandToNodeAfterEntrySuccessfullyWritten()
        {
            // Arrange
            var @event = new AppendEntriesRequested
            {
                PreviousLogTerm = 1,
                PreviousLogIndex = 5
            };

            var nodeData = new NodeData
            {
                CommitIndex = 20,
                CurrentTerm = 2
            };

            var writeDataBlocks = Substitute.For<IWriteDataBlocks>();
            var nodePublisher = new TestBufferPublisher<NodeCommandScheduled, NodeCommandResult>();

            var raftNode = Substitute.For<INode>();
            raftNode.Data.Returns(nodeData);

            var handler = new RpcLogTruncator(raftNode, writeDataBlocks, nodePublisher);

            // Act
            handler.OnNext(@event, 0L, false);

            // Assert
            writeDataBlocks.Received().WriteBlock(Arg.Any<byte[]>());
            nodePublisher.Events.Should().HaveCount(1);
            nodePublisher.Events[0].Command.Should().BeOfType<TruncateLog>();
            ((TruncateLog)nodePublisher.Events[0].Command).TruncateFromIndex.Should().Be(@event.PreviousLogIndex);
        }
    }
}
