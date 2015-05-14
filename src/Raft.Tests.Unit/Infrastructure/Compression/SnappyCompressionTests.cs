using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure.Compression;

namespace Raft.Tests.Unit.Infrastructure.Compression
{
    [TestFixture]
    public class SnappyCompressionTests
    {
        // Depending on the input data, Snappy may not be able to reduce input size during compression.
        // This string has enough re-occuring patters to be easily compressed.
        private const string RawDataString =
                "aaaaabbbbbcccddddeeeeffffaaaaabbaaaaa" +
                "bbbbbcccddddeeeeffffaaaaabbaaaaabbbbb" +
                "cccddddeeeeffffaaaaabbaabbaaaaabbbbbc";

        [Test]
        public void CanCompressData()
        {
            // Arrange
            var rawDataBytes = Encoding.Default.GetBytes(RawDataString);
            var snappyCompression = new SnappyCompression();

            // Act
            var compressedBytes = snappyCompression.Compress(rawDataBytes);

            // Assert
            compressedBytes.Length.Should()
                .BeLessThan(rawDataBytes.Length);
        }

        [Test]
        public void CanDecompressData()
        {
            // Arrange
            var rawDataBytes = Encoding.Default.GetBytes(RawDataString);
            var snappyCompression = new SnappyCompression();
            var compressedBytes = snappyCompression.Compress(rawDataBytes);

            // Act
            var decompressedBytes = snappyCompression.Decompress(compressedBytes);

            // Assert
            decompressedBytes.SequenceEqual(rawDataBytes)
                .Should().BeTrue();
        }
    }
}
