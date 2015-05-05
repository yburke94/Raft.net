using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

        private const int SizeOfHeaderVariables = sizeof(long);

        private const byte Eol = 0xFF; // End of list
        private const int SizeOfEol = sizeof(byte);

        private const ushort MaxIncrement = ushort.MaxValue; // Why... I Dont know why...

        const int BytesOffset = 0;
        const int TailOffset = sizeof(long);
        const int LengthOffset = sizeof(long) *2;

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
            _bytes += (SizeOfHeaderVariables*3) + SizeOfEol;
            ExtendBlockIfRequired(ref _blob, _bytes, 0L);

            WriteHeaderVariable(_blob, BytesOffset, _bytes);
            WriteHeaderVariable(_blob, TailOffset, _tail);
            WriteHeaderVariable(_blob, LengthOffset, _length);

            WriteEol(_blob, LengthOffset+SizeOfHeaderVariables);
        }

        public byte[] GetBytes()
        {
            var ret = new byte[_bytes];
            Array.Copy(_blob, ret, _bytes);
            return ret;
        }

        public static ZipList FromBytes(byte[] bytes)
        {
        }

        public bool HasEntries
        {
            get { return _length > 0; }
        }

        public byte[] Get(long idx)
        {

        }

        public void Push(byte[] bytes)
        {
            PushAll(new []{bytes});
        }

        public void PushAll(byte[][] byteBlocks)
        {
            var totalBytes = (byteBlocks.Length*(SizeOfHeaderVariables*2)) + byteBlocks.Sum(x => x.LongLength);
            ExtendBlockIfRequired(ref _blob, totalBytes, _bytes);

            foreach (var bytes in byteBlocks)
            {
                // Write List Header
                var oldTail = _tail;
                _tail = _bytes - SizeOfEol;

                var sizeOfEntry = (SizeOfHeaderVariables * 2) + bytes.Length;
                _bytes += sizeOfEntry;

                WriteHeaderVariable(_blob, BytesOffset, _bytes);

                WriteHeaderVariable(_blob, TailOffset, _tail);

                _length++;
                WriteHeaderVariable(_blob, LengthOffset, _length);

                // Write Entry
                WriteHeaderVariable(_blob, _tail, oldTail); // PrevLength
                WriteHeaderVariable(_blob, _tail + SizeOfHeaderVariables, bytes.LongLength); // DataLength

                var entryStart = _tail + (SizeOfHeaderVariables * 2);
                Write(_blob, entryStart, bytes); // Entry
            }

            // Write Eol
            WriteEol(_blob, _bytes - SizeOfEol);
        }

        public void Merge(ZipList zipList)
        {
        }

        public byte[][] Truncate(long entriesFromLast)
        {
        }

        public IEnumerable<byte[]> Iterate()
        {
            
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

        private static long ReadHeaderVariable(byte[] block, long offset)
        {
            var ret = new byte[SizeOfHeaderVariables];

            for (var i = 0; i < SizeOfHeaderVariables; i++)
                ret[i] = block[offset + i];

            return BitConverter.ToInt64(ret, 0);
        }

        private static void ExtendBlockIfRequired(ref byte[] block, long lengthAdded, long offset)
        {
            var maxChangeLength = offset + lengthAdded;
            if (maxChangeLength < block.LongLength) return;

            var newBlock = new byte[block.Length + MaxIncrement];
            Array.Copy(block, newBlock, block.Length);

            block = newBlock;
        }
    }

    private struct ZipListEntry
    {
        private long previousLength;
        private long length;

        private byte[] _entry;

        public byte[] GetBytes()
        {
        }

        public static ZipListEntry FromBytes(byte[] bytes)
        {
        }
    }

}
