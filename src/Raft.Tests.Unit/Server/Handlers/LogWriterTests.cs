using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Raft.Core;
using Raft.Infrastructure.Journaler;
using Raft.Server.Handlers;
using Raft.Server.Handlers.Contracts;
using Raft.Server.Log;
using Raft.Tests.Unit.TestData.Commands;

namespace Raft.Tests.Unit.Server.Handlers
{
    [TestFixture]
    public class LogWriterTests
    {
        [Test]
        public void DoesNotHandleInternalCommands()
        {
            // Act, Assert
            typeof(ISkipInternalCommands).IsAssignableFrom(typeof(LogWriter))
                .Should().BeTrue();
        }

        [Test]
        public void WillNotPerformWriteWhenNotAtTheEndOfTheBatch()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);

            var @event = TestEventFactory.GetCommandEvent();
            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(logRegister, journaler, node);

            // Act
            handler.OnNext(@event, 0, false);

            // Assert
            journaler.DidNotReceive().WriteBlocks(new [] {data});
        }

        [Test]
        public void WritesBlockWhenAtTheEndOfTheBatch()
        {
            // Arrange
            var data = BitConverter.GetBytes(1);

            var @event = TestEventFactory.GetCommandEvent();
            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(@event.Id, data);

            var handler = new LogWriter(logRegister, journaler, node);

            Expression<Predicate<byte[][]>> match = x => x[0].SequenceEqual(data);

            // Act
            handler.OnNext(@event, 0, true);

            // Assert
            journaler.Received().WriteBlocks(Arg.Is(match));
        }

        [Test]
        public void WritesPreviousBlocksInOrderWhenAtTheEndOfTheBatch()
        {
            // Arrange
            var data1 = BitConverter.GetBytes(1);
            var event1 = TestEventFactory.GetCommandEvent();

            var data2 = BitConverter.GetBytes(2);
            var event2 = TestEventFactory.GetCommandEvent();

            var data3 = BitConverter.GetBytes(3);
            var event3 = TestEventFactory.GetCommandEvent();

            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(event1.Id, data1);
            logRegister.AddEncodedLog(event2.Id, data2);
            logRegister.AddEncodedLog(event3.Id, data3);

            var handler = new LogWriter(logRegister, journaler, node);

            Expression<Predicate<byte[][]>> match = x =>
                x[0].SequenceEqual(data1) &&
                x[1].SequenceEqual(data2) &&
                x[2].SequenceEqual(data3);

            // Act
            handler.OnNext(event1, 0, false);
            handler.OnNext(event2, 1, false);
            handler.OnNext(event3, 2, true);

            // Assert
            journaler.Received().WriteBlocks(Arg.Is(match));
        }

        [Test]
        public void ResetsEntriesToBeWrittenUponFlushOfEachBatch()
        {
            // Arrange
            var data1 = BitConverter.GetBytes(1);
            var event1 = TestEventFactory.GetCommandEvent();

            var data2 = BitConverter.GetBytes(2);
            var event2 = TestEventFactory.GetCommandEvent();

            var data3 = BitConverter.GetBytes(3);
            var event3 = TestEventFactory.GetCommandEvent();

            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(event1.Id, data1);
            logRegister.AddEncodedLog(event2.Id, data2);
            logRegister.AddEncodedLog(event3.Id, data3);

            var handler = new LogWriter(logRegister, journaler, node);

            Expression<Predicate<byte[][]>> matchForBatch1 = x =>
                x[0].SequenceEqual(data1) &&
                x[1].SequenceEqual(data2);

            Expression<Predicate<byte[][]>> matchForBatch2 = x =>
                x[0].SequenceEqual(data3);

            // Act
            handler.OnNext(event1, 0, false);
            handler.OnNext(event2, 1, true);
            handler.OnNext(event3, 2, true);

            // Assert
            journaler.Received().WriteBlocks(Arg.Is(matchForBatch1));
            journaler.Received().WriteBlocks(Arg.Is(matchForBatch2));
        }

        [Test]
        public void CallsAddLogEntryOnRaftNodeForAllEntriesWrittenToLog()
        {
            // Arrange
            var data1 = BitConverter.GetBytes(1);
            var event1 = TestEventFactory.GetCommandEvent();

            var data2 = BitConverter.GetBytes(2);
            var event2 = TestEventFactory.GetCommandEvent();

            var data3 = BitConverter.GetBytes(3);
            var event3 = TestEventFactory.GetCommandEvent();

            var journaler = Substitute.For<IJournal>();
            var node = Substitute.For<IRaftNode>();

            var logRegister = new EncodedLogRegister(1);
            logRegister.AddEncodedLog(event1.Id, data1);
            logRegister.AddEncodedLog(event2.Id, data2);
            logRegister.AddEncodedLog(event3.Id, data3);

            var handler = new LogWriter(logRegister, journaler, node);

            // Act
            handler.OnNext(event1, 0, false);
            handler.OnNext(event2, 1, false);
            handler.OnNext(event3, 2, true);

            // Assert
            node.Received(3).AddLogEntry();
        }
    }
}
