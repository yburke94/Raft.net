using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure;
using Raft.Log;

namespace Raft.Tests.Unit.Log
{
    [TestFixture]
    public class LogCursorTests
    {
        [Test]
        public void CanGetPreviousLogEntryFromCursor()
        {
            // Arrange
            var entry = BitConverter.GetBytes(100);
            var ziplist = new Ziplist();
            ziplist.Push(entry);

            var logCursor = new LogCursor(new [] {ziplist},
                new Dictionary<long, long>() { { 1, 1 } });

            // Act
            var prev = logCursor.GetPreviousEntry();

            // Assert
            prev.SequenceEqual(entry)
                .Should().BeTrue();
        }

        [Test]
        public void ReturnIfEntryIsCompressedInMemory()
        {
            // Arrange
            var oldTermZiplist = new Ziplist();
            oldTermZiplist.Push(BitConverter.GetBytes(233));

            var currTermZiplist = new Ziplist();
            currTermZiplist.Push(BitConverter.GetBytes(233));

            var logCursor = new LogCursor(new[] {oldTermZiplist, currTermZiplist},
                new Dictionary<long, long>(){{1,1}, {2,2}});

            // Act
            var entry1Compressed = logCursor.IsCompressed(1);
            var entry2Compressed = logCursor.IsCompressed(2);

            // Assert
            entry1Compressed.Should().BeTrue("it is part of an old term.");
            entry2Compressed.Should().BeFalse("it is part of the current term.");
        }
    }
}
