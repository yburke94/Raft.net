using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Raft.Infrastructure
{
    /// <summary>
    /// A simplified implementation of the Ziplist design used in Redis.
    /// Whilst Redis' implementation is smarter about memory consumption when dealing with lengths/offsets,
    /// this implementation just encodes all length/offset metadata as 32bit integers.
    /// Also, entries are simple byte arrays so entry headers do not contain encoding information.
    /// The rest of the layout is more or less the same.
    /// </summary>
    /// <remarks>
    /// This object is NOT THREAD SAFE! Operation on this list should be performed sequentially!
    /// </remarks>
    public class ZipList
    {
        private const int SizeOfHeaderVariable = sizeof(int);

        private const int SizeOfZipListHeader = (SizeOfHeaderVariable * 3);
        private const int SizeOfEntryHeader = (SizeOfHeaderVariable*2);

        private const byte Eol = 0xFF; // End of list
        private const int SizeOfEol = sizeof(byte);

        private const ushort MaxIncrement = ushort.MaxValue;

        private const int BytesOffset = 0;
        private const int TailOffset = SizeOfHeaderVariable;
        private const int LengthOffset = SizeOfHeaderVariable *2;

        private int _bytes;
        private int _tail;
        private int _length;

        private byte[] _blob;

        /// <summary>
        /// No of entries.
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Size of list in bytes.
        /// </summary>
        public int SizeOfList
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Size of blob in bytes.
        /// </summary>
        public int SizeInMemory
        {
            get { return _blob.Length; }
        }

        /// <summary>
        /// Intiializes an empty Ziplist object.
        /// </summary>
        public ZipList()
        {
            _bytes = 0;
            _tail = 0;
            _length = 0;

            _blob = new byte[0];

            Init();
        }

        private ZipList(int bytes, int tail, int length, byte[] blob)
        {
            _bytes = bytes;
            _tail = tail;
            _length = length;

            _blob = blob;
        }

        /// <summary>
        /// Constructs a ZipList structure given a valid array of bytes.
        /// </summary>
        public static ZipList FromBytes(byte[] zipListBlob)
        {
            var bytes = ReadHeaderVariable(zipListBlob, BytesOffset);
            var tail = ReadHeaderVariable(zipListBlob, TailOffset);
            var length = ReadHeaderVariable(zipListBlob, LengthOffset);

            var eol = Read(zipListBlob, bytes - 1, SizeOfEol);
            if (eol[0] != Eol)
                throw new ArgumentException(
                    "The passed ziplist bytes are invalid. " +
                    "Ensure the bytes passed have not been corrupted.", "zipListBlob");

            return new ZipList(bytes, tail, length, zipListBlob);
        }

        /// <summary>
        /// Resizes the underlying blob to occupy the same space as the Ziplist itself.
        /// </summary>
        public void Resize()
        {
            _blob = GetBytes();
        }

        /// <summary>
        /// Returns a copy of the ZipList represented as an array of bytes.
        /// </summary>
        public byte[] GetBytes()
        {
            var ret = new byte[_bytes];
            Array.Copy(_blob, ret, _bytes);
            return ret;
        }

        /// <summary>
        /// Returns whether the ZipList structure has any entries.
        /// </summary>
        public bool HasEntries
        {
            get { return _length > 0; }
        }

        /// <summary>
        /// Pushes an entry to the end of the ZipList.
        /// </summary>
        public void Push(byte[] bytes)
        {
            PushAll(new []{bytes});
        }

        /// <summary>
        /// Pushes multiple entries to the end of the ZipList.
        /// </summary>
        public void PushAll(byte[][] byteBlocks)
        {
            var totalBytes = (byteBlocks.Length*SizeOfEntryHeader) + byteBlocks.Sum(x => x.Length);
            ExtendBlockIfRequired(ref _blob, totalBytes, _bytes);

            foreach (var bytes in byteBlocks)
            {
                // Write List Header
                var oldTail = _tail;
                _tail = _bytes - SizeOfEol;

                var sizeOfEntry = SizeOfEntryHeader + bytes.Length;
                _bytes += sizeOfEntry;

                WriteHeaderVariable(_blob, BytesOffset, _bytes);

                WriteHeaderVariable(_blob, TailOffset, _tail);

                _length++;
                WriteHeaderVariable(_blob, LengthOffset, _length);

                // Write Entry
                var entry = new ZipListEntry(oldTail, _tail, bytes);
                Write(_blob, _tail, entry.GetBytes());
            }

            // Write Eol
            WriteEol(_blob, _bytes - SizeOfEol);
        }

        /// <summary>
        /// Merges the current ZipList with the Ziplist provided.
        /// All entries in the provided ZipList will be appended to the current with the order preserved.
        /// </summary>
        public void Merge(ZipList zipList)
        {
            var idx = 0;
            var entries = new byte[zipList.Length][];
            foreach (var entry in zipList.Reader())
            {
                entries[idx] = entry.Data;
                idx++;
            }
            
            PushAll(entries);
        }

        /// <summary>
        /// Removes entries from the tail of the list.
        /// </summary>
        /// <param name="entriesToRemove">
        /// When equal to 1(default value), a pop operation will be performed at the tail.
        /// When greater than 1, the amount specified will be removed from the list.
        /// </param>
        /// <returns>The entries removed from the ZipList in the order it existed in the list.</returns>
        public ZipListEntry[] Truncate(int entriesToRemove = 1)
        {
            if (entriesToRemove < 1)
                throw new ArgumentException("Entries to remove must not be less than 1.");

            if (entriesToRemove > _length)
                throw new ArgumentException("Entries to remove cannot be greater than length.");

            var entriesToReturn = new ZipListEntry[entriesToRemove];

            var nextEntryStart = _tail;
            var offsetsCalculated = 0;
            
            while (offsetsCalculated < entriesToRemove && nextEntryStart > 0)
            {
                var entry = Get(nextEntryStart);
                entriesToReturn[offsetsCalculated] = entry;

                nextEntryStart = entry.PreviousOffset;
                offsetsCalculated++;
            }

            var nextEntryLength = nextEntryStart == 0
                ? 0
                : ReadHeaderVariable(_blob, nextEntryStart + SizeOfHeaderVariable);

            var eolOffset = nextEntryStart == 0
                ? SizeOfZipListHeader
                : nextEntryStart + SizeOfEntryHeader + nextEntryLength;

            var oldBytes = _bytes;

            _length = _length-entriesToRemove;
            _bytes = eolOffset + 1;
            _tail = nextEntryStart;

            // Write new ZipList Header
            WriteHeaderVariable(_blob, BytesOffset, _bytes);
            WriteHeaderVariable(_blob, TailOffset, _tail);
            WriteHeaderVariable(_blob, LengthOffset, _length);

            // Set new EOL & Overwrite the remaining bytes.
            WriteEol(_blob, eolOffset);

            var delta = oldBytes - _bytes;
            var nullBytes = new byte[delta];
            Write(_blob, _bytes, nullBytes);

            // Return in the order it existed in the list.
            Array.Reverse(entriesToReturn);
            return entriesToReturn;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable"/> that will allow you to traverse the list from front to back.
        /// </summary>
        public IEnumerable<ZipListEntry> Reader()
        {
            return new ZipListEnumerator(this);
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable"/> that will allow you to traverse the list from back to front.
        /// </summary>
        public IEnumerable<ZipListEntry> ReverseReader()
        {
            return new ZipListEnumerator(this, true);
        }

        /// <summary>
        /// Gets the Head(first entry) of the list.
        /// </summary>
        public ZipListEntry Head()
        {
            return HasEntries
                ? Get(SizeOfZipListHeader)
                : null;
        }

        /// <summary>
        /// Gets the Tail(last entry) of the list.
        /// </summary>
        public ZipListEntry Tail()
        {
            return HasEntries
                ? Get(_tail)
                : null;
        }

        /// <summary>
        /// Gets the entry immediately preceeding the supplied entry.
        /// If there is no preceeding entry, null will be returned.
        /// </summary>
        public ZipListEntry Prev(ZipListEntry entry)
        {
            if (!HasEntries)
                throw new InvalidOperationException("No items have been added to the ZipList.");

            if (entry.PreviousOffset == 0) return null; // Entry passed was the first entry.

            var prev = Get(entry.PreviousOffset);

            // Read the variable after the previous entry to ensure the passed entry is valid.
            var currEntryPrevOffset = ReadHeaderVariable(_blob, prev.Offset + SizeOfEntryHeader + prev.Length);
            if (currEntryPrevOffset != entry.PreviousOffset)
                throw new InvalidOperationException("The entry passed was invalid. The previous offset pointed to invalid data.");

            return prev;
        }

        /// <summary>
        /// Gets the entry immediately following the supplied entry.
        /// If there is no following entry, null will be returned.
        /// </summary>
        public ZipListEntry Next(ZipListEntry entry)
        {
            if (!HasEntries)
                throw new InvalidOperationException("No items have been added to this ZipList.");

            if (_bytes-1 <= entry.Offset || SizeOfZipListHeader > entry.Offset)
                throw new IndexOutOfRangeException("Entry offset did not fall within range for entries.");

            // Read 'PreviousOffset' from current entry in blob to ensure the passed entry is valid.
            var currPrevOffset = ReadHeaderVariable(_blob, entry.Offset);
            if (currPrevOffset != entry.PreviousOffset)
                throw new InvalidOperationException("The entry passed was invalid. The previous offset pointed to invalid data.");

            var nextEntryOffset = entry.Offset + SizeOfEntryHeader + entry.Length;
            return nextEntryOffset == _bytes - 1
                ? null // Entry passed was the last entry.
                : Get(nextEntryOffset);
        }

        private ZipListEntry Get(int offset)
        {
            if (_bytes - 1 <= offset || SizeOfZipListHeader > offset)
                throw new IndexOutOfRangeException("Supplied offset does not fall within range for entries.");

            var prevEntryOffset = ReadHeaderVariable(_blob, offset);
            var entryLength = ReadHeaderVariable(_blob, offset + SizeOfHeaderVariable);
            var entryBytes = Read(_blob, offset + SizeOfEntryHeader, entryLength);

            return new ZipListEntry(prevEntryOffset, offset, entryBytes);
        }

        private void Init()
        {
            _bytes += SizeOfZipListHeader + SizeOfEol;
            ExtendBlockIfRequired(ref _blob, _bytes, 0);

            WriteHeaderVariable(_blob, BytesOffset, _bytes);
            WriteHeaderVariable(_blob, TailOffset, _tail);
            WriteHeaderVariable(_blob, LengthOffset, _length);

            WriteEol(_blob, SizeOfZipListHeader);
        }

        private static void WriteHeaderVariable(byte[] block, int offset, int value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            Write(block, offset, valueAsBytes);
        }

        private static void WriteEol(byte[] block, int offset)
        {
            Write(block, offset, new []{Eol});
        }

        private static void Write(byte[] block, int offset, byte[] value)
        {
            Array.Copy(value, 0, block, offset, value.Length);
        }

        private static byte[] Read(byte[] block, int offset, int length)
        {
            var ret = new byte[length];

            for (var i = 0; i < length; i++)
                ret[i] = block[offset + i];

            return ret;
        }

        private static int ReadHeaderVariable(byte[] block, int offset)
        {
            return BitConverter.ToInt32(Read(block, offset, SizeOfHeaderVariable), 0);
        }

        private static void ExtendBlockIfRequired(ref byte[] block, int lengthAdded, int addingFrom)
        {
            var maxChangeLength = addingFrom + lengthAdded;
            var delta = maxChangeLength - block.Length;
            if (delta < 0) return;

            // Try to simply double the array size. If that exceeds MaxIncrement, use MaxIncrement.
            var desiredIncrement = Math.Min(block.Length, MaxIncrement);

            // If the desired increment size will not fit the amount to be added,
            // append the amount to be added to current size.
            var increment = desiredIncrement < delta ? delta : desiredIncrement;

            var newBlock = new byte[block.Length + increment];
            Array.Copy(block, newBlock, block.Length);

            block = newBlock;
        }

        private class ZipListEnumerator : IEnumerator<ZipListEntry>, IEnumerable<ZipListEntry>
        {
            private readonly bool _backToFront;

            private int _idx;
            private ZipList _list;
            private ZipListEntry _current;

            public ZipListEnumerator(ZipList list, bool backToFront = false)
            {
                _list = list;
                _backToFront = backToFront;

                Reset();
            }

            public bool MoveNext()
            {
                _idx = _backToFront ? _idx - 1 : _idx + 1;

                if ((!_backToFront && _idx == _list.Length) || (_backToFront && _idx == -1L))
                    return false;

                if (_current == null)
                {
                    if (_idx == _list.Length - 1 && _backToFront)
                        _current = _list.Tail();

                    if (_idx == 0L && !_backToFront)
                        _current = _list.Head();
                }
                else
                {
                    _current = _backToFront
                        ? _list.Prev(_current)
                        : _list.Next(_current);
                }

                return _current != null;
            }

            public void Reset()
            {
                _idx = _backToFront ? _list.Length : -1;
                _current = null;
            }

            public ZipListEntry Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public IEnumerator<ZipListEntry> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Dispose()
            {
                _list = null;
                _current = null;
            }
        }

        /// <summary>
        /// Represents an entry endoded in a Ziplist.
        /// </summary>
        public class ZipListEntry
        {
            /// <summary>
            /// The offset for the entry immediately preceeding the current entry.
            /// </summary>
            public int PreviousOffset { get; private set; }

            /// <summary>
            /// The length of the data for this entry
            /// </summary>
            public int Length { get; private set; }

            /// <summary>
            /// The data for this entry.
            /// </summary>
            public byte[] Data { get; private set; }

            /// <summary>
            /// The offset for this entry
            /// </summary>
            /// <remarks>
            /// This information is held in memory only and is not encoded in the ZipList.
            /// </remarks>
            public int Offset { get; private set; }

            public ZipListEntry(int prevOffset, int offset, byte[] data)
            {
                PreviousOffset = prevOffset;
                Length = data.Length;
                Data = data;

                Offset = offset;
            }

            public byte[] GetBytes()
            {
                var entrySize = SizeOfEntryHeader + Length;
                var asBytes = new byte[entrySize];

                WriteHeaderVariable(asBytes, 0, PreviousOffset);
                WriteHeaderVariable(asBytes, SizeOfHeaderVariable, Length);
                Write(asBytes, SizeOfEntryHeader, Data);

                return asBytes;
            }
        }
    }
}
