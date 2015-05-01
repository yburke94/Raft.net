using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Persistance.Journaler.Transformers;

namespace Raft.Persistance.Journaler.Tests.Unit.Transformers
{
    [TestFixture]
    public class EntryDataTests
    {
        [Test]
        public void PrependsEntryDataLengthToBytes()
        {
            // Arrange
            var entry = BitConverter.GetBytes(100);
            var entryData = new EntryData();

            // Act
            var result = entryData.Transform(entry, new Dictionary<string, string>());

            // Assert
            result.Length.Should().Be(entry.Length + sizeof (int));
            result.Take(sizeof (int))
                .SequenceEqual(BitConverter.GetBytes(entry.Length))
                .Should().BeTrue();
        }
    }
}
