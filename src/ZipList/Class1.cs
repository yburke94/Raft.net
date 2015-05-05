using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZipList
{
    public class Class1
    {
        /*
         <bytes><tail><len><entry><entry><end>
         * 
         * <bytes> is an unsigned integer to hold the number of bytes that the
 * ziplist occupies. This value needs to be stored to be able to resize the
 * entire structure without the need to traverse it first.
 *
 * <tail> is the offset to the last entry in the list. This allows a pop
 * operation on the far side of the list without the need for full traversal.
 *
 * <len> is the number of entries.When this value is larger than 2**16-2,
 * we need to traverse the entire list to know how many items it holds.
 *
 * <end> is a single byte special value, equal to 255, which indicates the
 * end of the list.
         * 
         * 
         * <startOfPRevious><{strEncode/int**}><entryLength><entry>
         * Every entry in the ziplist is prefixed by a header that contains two pieces
 * of information. First, the length of the previous entry is stored to be
 * able to traverse the list from back to front. Second, the encoding with an
 * optional string length of the entry itself is stored.
         
         *Length (Entries)
         *Size (Blob Length)
         * 
         * *Pop
         * 
         * Push
         * PushAll
         * Truncate
         * Iterate
         */

        /*
         Orig:
         * 
         * HEAD | Tail
         * 
         * New
         * Merge
         * Push
         * Index??
         * Next/PRev {Iterator}
         * Get(len)
         * Insert(len)
         * Delete
         * DeleteRange
         * Compare
         * Find
         */
    }

    /// <summary>
    /// Impl of Ziplist inspired by Redis.
    /// </summary>
    public class ZipList
    {
        private long _bytes;
        private long _tail;
        private long _length;

        private const int SizeOfHeaderVariable = sizeof(long);

        private const int SizeOfZipListHeader = (SizeOfHeaderVariable * 3);
        private const int SizeOfEntryHeader = (SizeOfHeaderVariable*2);

        private const byte Eol = 0xFF; // End of list
        private const int SizeOfEol = sizeof(byte);

        // TODO: Make increments smart based on length, throughput e.t.c
        private const ushort MaxIncrement = ushort.MaxValue;

        const int BytesOffset = 0;
        const int TailOffset = SizeOfHeaderVariable;
        const int LengthOffset = SizeOfHeaderVariable *2;

        private byte[] _blob;

        /// <summary>
        /// No of entries.
        /// </summary>
        public long Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Size of list in bytes.
        /// </summary>
        public long SizeOfList
        {
            get { return _bytes; }
        }

        /// <summary>
        /// Size of blob in bytes.
        /// </summary>
        public long SizeInMemory
        {
            get { return _blob.LongLength; }
        }

        public ZipList()
        {
            _blob = new byte[MaxIncrement];

            _bytes = 0L;
            _tail = 0L;
            _length = 0L;

            Init();
        }

        private void Init()
        {
            _bytes += SizeOfZipListHeader + SizeOfEol;
            ExtendBlockIfRequired(ref _blob, _bytes, 0L);

            WriteHeaderVariable(_blob, BytesOffset, _bytes);
            WriteHeaderVariable(_blob, TailOffset, _tail);
            WriteHeaderVariable(_blob, LengthOffset, _length);

            WriteEol(_blob, SizeOfZipListHeader);
        }

        public byte[] GetBytes()
        {
            var ret = new byte[_bytes];
            Array.Copy(_blob, ret, _bytes);
            return ret;
        }

        public static ZipList FromBytes(byte[] bytes)
        { // TODO
        }

        public bool HasEntries
        {
            get { return _length > 0; }
        }

        // TODO: This is not sequential memory access. Make it sequential!
        public ZipListEntry GetPrev(ZipListEntry entry)
        {
            if (_length == 0)
                throw new InvalidOperationException("No items have been added to thie ZipList.");

            if (_bytes <= entry.PreviousOffset || SizeOfZipListHeader > entry.PreviousOffset)
                throw new IndexOutOfRangeException("Previous entry offset did not fall within range for entries.");

            var prevEntryOffset = ReadHeaderVariable(_blob, entry.PreviousOffset);
            var entryLength = ReadHeaderVariable(_blob, entry.PreviousOffset + SizeOfHeaderVariable);
            var entryBytes = Read(_blob, entry.PreviousOffset + SizeOfEntryHeader, entryLength);

            var nextEntryPrevOffset = ReadHeaderVariable(_blob, entry.PreviousOffset + SizeOfEntryHeader + entryLength);
            if (nextEntryPrevOffset != entry.PreviousOffset)
                throw new InvalidOperationException("The entry passed was invalid. The previous offset pointed to invalid data.");

            return new ZipListEntry(prevEntryOffset, entryBytes);
        }

        // TODO: This is not sequential memory access. Make it sequential!
        public ZipListEntry GetNext(ZipListEntry entry)
        {
            if (_length == 0)
                throw new InvalidOperationException("No items have been added to thie ZipList.");

            if (_bytes <= entry.PreviousOffset || SizeOfZipListHeader > entry.PreviousOffset)
                throw new IndexOutOfRangeException("Previous entry offset did not fall within range for entries.");

            var prev = GetPrev(entry);
            var currentEntryOffset = entry.PreviousOffset + SizeOfEntryHeader + prev.Length;
            var nextEntryOffset = currentEntryOffset + SizeOfEntryHeader + entry.Length;
            return GetPrev(new ZipListEntry(nextEntryOffset, new byte[0]));
        }

        public void Push(byte[] bytes)
        {
            PushAll(new []{bytes});
        }

        public void PushAll(byte[][] byteBlocks)
        {
            var totalBytes = (byteBlocks.Length*SizeOfEntryHeader) + byteBlocks.Sum(x => x.LongLength);
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
                var entry = new ZipListEntry(oldTail, bytes);
                Write(_blob, _tail, entry.GetBytes());
            }

            // Write Eol
            WriteEol(_blob, _bytes - SizeOfEol);
        }

        public void Merge(ZipList zipList)
        {
            var entries = zipList.Reader().Select(x => x.Entry).ToArray();
            PushAll(entries);
        }

        /// <summary>
        /// When entries from last = 0, a pop operation will be performed.
        /// </summary>
        /// <returns>The entries removed from the ZipList.</returns>
        public ZipListEntry[] Truncate(long entriesFromLast = 0)
        {
            // if (_tail == 0) throw!
        }

        public IEnumerable<ZipListEntry> Reader()
        {
            return new ZipListEnumerator(this);
        }

        public IEnumerable<ZipListEntry> BackToFrontReader()
        {
            return new ZipListEnumerator(this, true);
        }

        private static void WriteHeaderVariable(byte[] block, long offset, long value)
        {
            var valueAsBytes = BitConverter.GetBytes(value);
            Write(block, offset, valueAsBytes);
        }

        private static void WriteEol(byte[] block, long offset)
        {
            Write(block, offset, new []{Eol});
        }

        private static void Write(byte[] block, long offset, byte[] value)
        {
            Array.Copy(value, 0, block, offset, value.Length);
        }

        private static byte[] Read(byte[] block, long offset, long length)
        {
            var ret = new byte[length];

            for (var i = 0; i < length; i++)
                ret[i] = block[offset + i];

            return ret;
        }

        private static long ReadHeaderVariable(byte[] block, long offset)
        {
            return BitConverter.ToInt64(Read(block, offset, SizeOfHeaderVariable), 0);
        }

        private static void ExtendBlockIfRequired(ref byte[] block, long lengthAdded, long offset)
        {
            var maxChangeLength = offset + lengthAdded;
            if (maxChangeLength < block.LongLength) return;

            var newBlock = new byte[block.Length + MaxIncrement];
            Array.Copy(block, newBlock, block.Length);

            block = newBlock;
        }

        private class ZipListEnumerator : IEnumerator<ZipListEntry>, IEnumerable<ZipListEntry>
        {
            private readonly ZipList _list;
            private readonly bool _backToFront;

            private long _idx = 0L;

            public ZipListEnumerator(ZipList list, bool backToFront = false)
            {
                _list = list;
                _backToFront = backToFront;
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
                _idx = 0L;
            }

            public ZipListEntry Current
            {
                get { return null; }
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

            public void Dispose() { }
        }

        public class ZipListEntry
        {
            public long PreviousOffset { get; private set; }
            public long Length { get; private set; }
            public byte[] Entry { get; private set; }

            public ZipListEntry(long prevOffset, byte[] entry)
            {
                PreviousOffset = prevOffset;
                Length = entry.Length;
                Entry = entry;
            }

            public byte[] GetBytes()
            {
                var entrySize = SizeOfEntryHeader + Length;
                var asBytes = new byte[entrySize];

                WriteHeaderVariable(asBytes, 0, PreviousOffset);
                WriteHeaderVariable(asBytes, SizeOfHeaderVariable, Length);
                Write(asBytes, SizeOfEntryHeader, Entry);

                return asBytes;
            }
        }
    }
}
