using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Raft.Persistance.Journaler.Extensions;
using Raft.Persistance.Journaler.Transformers;

namespace Raft.Persistance.Journaler.Tests.Unit.Transformers
{
    [TestFixture]
    public class EntryMetadataTests
    {
        [Test]
        public void PrependsStringifiedMetadataBytesToEntry()
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { "Type", "Foo" } };
            var metadataStringified = metadata.Stringify();
            var metadataBytes = Encoding.Default.GetBytes(metadataStringified);

            var entry = BitConverter.GetBytes(100);

            var entrymetadata = new EntryMetadata();

            // Act
            var result = entrymetadata.Transform(entry, metadata);

            // Assert
            result.Length.Should().Be(entry.Length + metadataBytes.Length + sizeof(int) /*metadata length bytes*/);

            result.Skip(sizeof(int)).Take(metadataBytes.Length)
                .SequenceEqual(metadataBytes)
                .Should().BeTrue();
        }

        [Test]
        public void WritesStringifiedMetadataBytesLengthToStartOfEntry()
        {
            // Arrange
            var metadata = new Dictionary<string, string> { { "Type", "Foo" } };
            var metadataStringified = metadata.Stringify();
            var metadataBytes = Encoding.Default.GetBytes(metadataStringified);

            var entry = BitConverter.GetBytes(100);

            var entrymetadata = new EntryMetadata();

            // Act
            var result = entrymetadata.Transform(entry, metadata);

            // Assert
            result.Length.Should().Be(entry.Length + metadataBytes.Length + sizeof(int) /*metadata length bytes*/);

            result.Take(sizeof(int))
                .SequenceEqual(BitConverter.GetBytes(metadataBytes.Length))
                .Should().BeTrue();
        }
    }
}
