using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Raft.Infrastructure;

namespace Raft.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class ZiplistTests
    {
        private const int ZiplistHeaderSize = sizeof (int)*3;
        private const int ZiplistEntryHeaderSize = sizeof(int) * 2;
        private const int ZiplistEolSize = sizeof(byte);

        [Test]
        public void CanCreateZiplistAndInitializeCorrectValues()
        {
            // Act
            var ziplist = new ZipList();

            // Assert
            ziplist.SizeOfList.Should().Be(ZiplistHeaderSize + ZiplistEolSize);
            ziplist.Length.Should().Be(0);
        }

        [Test]
        public void CanPushEntryToZiplist()
        {
            // Arrange
            var entry = BitConverter.GetBytes(102L);
            var ziplist = new ZipList();

            // Act
            ziplist.Push(entry);

            // Assert
            ziplist.Length.Should().Be(1);

            ziplist.SizeOfList.Should()
                .Be(ZiplistHeaderSize + ZiplistEntryHeaderSize + entry.Length + ZiplistEolSize);
        }

        [TestCase(100000)]
        [TestCase(1000000)]
        [TestCase(10000000)]
        public void CanPushEntriesToZiplist(int noOfEntries)
        {
            // Arrange
            var entry = BitConverter.GetBytes(10L);
            var entries = new byte[noOfEntries][];
            for (var i = 0; i < noOfEntries; i++)
                entries[i] = entry;

            var ziplist = new ZipList();

            var combinedEntryHeaderLength = ZiplistEntryHeaderSize*noOfEntries;
            var combinedEntryLength = entry.Length * noOfEntries;

            // Act
            ziplist.PushAll(entries);

            // Assert
            ziplist.Length.Should().Be(noOfEntries);

            ziplist.SizeOfList.Should().Be(
                ZiplistHeaderSize + 
                combinedEntryHeaderLength + 
                combinedEntryLength + 
                ZiplistEolSize);
        }

        [Test]
        public void CanMergeZiplists()
        {
            // Arrange
            var entry = BitConverter.GetBytes(10L);

            var ziplist1 = new ZipList();
            ziplist1.Push(entry);

            var ziplist2 = new ZipList();
            ziplist2.Push(entry);
            ziplist2.Push(entry);

            const int combinedEntryHeaderLength = ZiplistEntryHeaderSize*3;
            var combinedEntryLength = entry.Length * 3;

            // Act
            ziplist1.Merge(ziplist2);

            // Assert
            ziplist1.Length.Should().Be(3);

            ziplist1.SizeOfList.Should().Be(
                ZiplistHeaderSize +
                combinedEntryHeaderLength +
                combinedEntryLength +
                ZiplistEolSize);
        }

        [Test]
        public void CanPopEntry()
        {
            // Arrange
            var entry = BitConverter.GetBytes(10L);

            var ziplist = new ZipList();
            ziplist.Push(entry);
            ziplist.Push(entry);
            ziplist.Push(entry);

            ziplist.Length.Should().Be(3);

            // Act
            var result = ziplist.Truncate();

            // Assert
            ziplist.Length.Should().Be(2);
            result.Length.Should().Be(1);

            result[0].Data.SequenceEqual(entry)
                .Should().BeTrue();
        }

        [Test]
        public void CanTruncateMultipleEntries()
        {
            // Arrange
            var entriesToTruncate = new[]
            {
                BitConverter.GetBytes(1234324),
                BitConverter.GetBytes(987645),
                BitConverter.GetBytes(3969302)
            };

            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(10L));
            ziplist.Push(BitConverter.GetBytes(10L));
            ziplist.Push(entriesToTruncate[0]);
            ziplist.Push(entriesToTruncate[1]);
            ziplist.Push(entriesToTruncate[2]);

            ziplist.Length.Should().Be(5);

            // Act
            var result = ziplist.Truncate(entriesToTruncate.Length);

            // Assert
            ziplist.Length.Should().Be(2);
            result.Length.Should().Be(entriesToTruncate.Length);

            var resultData = result.Select(x => x.Data).ToArray();
            for (var i = 0; i < entriesToTruncate.Length; i++)
                resultData[i].SequenceEqual(entriesToTruncate[i]).Should().BeTrue();
        }

        [Test]
        public void CanGetHeadOfList()
        {
            // Arrange
            var head = BitConverter.GetBytes(8723648372L);

            var ziplist = new ZipList();
            ziplist.Push(head);
            ziplist.Push(BitConverter.GetBytes(11L));

            // Act
            var result = ziplist.Head();

            // Assert
            result.Data.SequenceEqual(head)
                .Should().BeTrue();
        }

        [Test]
        public void CanGetTailOfList()
        {
            // Arrange
            var tail = BitConverter.GetBytes(5423165451L);

            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(11L));
            ziplist.Push(tail);

            // Act
            var result = ziplist.Tail();

            // Assert
            result.Data.SequenceEqual(tail)
                .Should().BeTrue();
        }

        [Test]
        public void CanGetNextEntryInList()
        {
            // Arrange
            var middle = BitConverter.GetBytes(5423165451L);

            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(11L));
            ziplist.Push(middle);
            ziplist.Push(BitConverter.GetBytes(124L));

            var head = ziplist.Head();

            // Act
            var result = ziplist.Next(head);

            // Assert
            result.Data.SequenceEqual(middle)
                .Should().BeTrue();
        }

        [Test]
        public void CanGetPreviousEntryInList()
        {
            // Arrange
            var middle = BitConverter.GetBytes(5423165451L);

            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(11L));
            ziplist.Push(middle);
            ziplist.Push(BitConverter.GetBytes(124L));

            var tail = ziplist.Tail();

            // Act
            var result = ziplist.Prev(tail);

            // Assert
            result.Data.SequenceEqual(middle)
                .Should().BeTrue();
        }

        [TestCase(12462)]
        [TestCase(389754)]
        [TestCase(3457943)]
        public void CanTraverseListFrontToBack(int numberOfItems)
        {
            // Arrange
            var random = new Random();

            var data = new byte[numberOfItems][];
            for (var i = 0; i < numberOfItems; i++)
                data[i] = (BitConverter.GetBytes(random.Next()));

            var ziplist = new ZipList();
            ziplist.PushAll(data);

            // Act
            var reader = ziplist.Reader();

            // Assert
            var idx = 0;
            foreach (var entry in reader)
            {
                entry.Data.SequenceEqual(data[idx]).Should().BeTrue();
                idx++;
            }
        }

        [TestCase(12462)]
        [TestCase(389754)]
        [TestCase(3457943)]
        public void CanTraverseListBackToFront(int numberOfItems)
        {
            // Arrange
            var random = new Random();

            var data = new byte[numberOfItems][];
            for (var i = 0; i < numberOfItems; i++)
                data[i] = (BitConverter.GetBytes(random.Next()));

            var ziplist = new ZipList();
            ziplist.PushAll(data);

            Array.Reverse(data);

            // Act
            var reader = ziplist.ReverseReader();

            // Assert
            var idx = 0;
            foreach (var entry in reader)
            {
                entry.Data.SequenceEqual(data[idx]).Should().BeTrue();
                idx++;
            }
        }

        [Test]
        public void GetBytesReturnsValidByteArray()
        {
            // Arrange
            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(100L));

            // Act
            var bytes = ziplist.GetBytes();

            // Assert
            BitConverter.ToInt32(bytes.Take(4).ToArray(), 0)
                .Should().Be(ziplist.SizeOfList);

            BitConverter.ToInt32(bytes.Skip(8).Take(4).ToArray(), 0) // Missing out tail.
                .Should().Be(ziplist.Length);

            bytes.Last().Should().Be(0xFF);
        }

        [Test]
        public void FromBytesReturnsZipListGivenValidByteArray()
        {
            // Arrange
            var ziplist = new ZipList();
            ziplist.Push(BitConverter.GetBytes(100L));
            var bytes = ziplist.GetBytes();

            // Act
            var ziplist2 = ZipList.FromBytes(bytes);

            // Assert
            ziplist2.Length.Should().Be(ziplist.Length);
            ziplist2.SizeOfList.Should().Be(ziplist.SizeOfList);
        }

        [Test]
        public void CanResizeAutoExpandingList()
        {
            // Arrange
            var ziplist = new ZipList();
            ziplist.SizeInMemory.Should().Be(13); // Size of header + eol;

            // The entry will be 16 bytes. The ziplist will try to double the size of the array (current size = 13)
            // but that will not be large enough to accomodate the new bytes. As a result it will increase the array
            // byt the size of the new bytes.
            ziplist.Push(BitConverter.GetBytes(45634L));
            ziplist.SizeInMemory.Should().Be(29);

            // The entry will be 16 bytes. The ziplist will try to double the size of the array (current size = 29).
            // That will be large enough to accomodate the new entry. Leaving 13 trailing bytes;
            ziplist.Push(BitConverter.GetBytes(4357634L));
            ziplist.SizeInMemory.Should().Be(58);

            // The actual size of the ziplist should be 45.
            ziplist.SizeOfList.Should().Be(45);

            // Act
            ziplist.Resize();

            // Assert
            ziplist.SizeInMemory.Should().Be(ziplist.SizeOfList);
        }
    }
}
