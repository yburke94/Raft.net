using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
         
         *Length
         * 
         * *Pop
         * 
         * *Push
         * *PushAll
         * 
         * *Truncate
         * 
         * *Iterate
         */
    }

    /// <summary>
    /// Impl of Ziplist inspired by Redis.
    /// </summary>
    public class ZipList
    {
        private ulong _bytes;
        private ulong _tail;
        private ulong _length;

        private const byte Eol = 0xFF; // End of list
        private const ushort MaxIncrement = ushort.MaxValue; // Why... I Dont know why...

        const int BytesOffset = 2 << 0;
        const int TailOffset = 2 << (sizeof(ulong)*8);
        const int LengthOffset = 2 << ((sizeof(ulong) * 8) * 2);

        private byte[] _list;

        public ulong Length
        {
            get { return _length; }
        }

        public ZipList()
        {
            _list = new byte[MaxIncrement];

            _bytes = 0L;
            _tail = 0L;
            _length = 0L;

            Init();
        }

        private void Init()
        {
            if (_length != 0) throw new Exception(); // TODO: errm!!!

            const int bytesToAdd = (sizeof (long)*3) + sizeof (byte); // <bytes><tail><length><eol>
            ExtendBlockIfRequired(ref _list, bytesToAdd, BytesOffset);

            Array.Copy(BitConverter.GetBytes(_bytes), 0, _list, BytesOffset, sizeof(long));
            Array.Copy(BitConverter.GetBytes(_tail), 0, _list, TailOffset, sizeof(long));
            Array.Copy(BitConverter.GetBytes(_length), 0, _list, LengthOffset, sizeof(long));
            Array.Copy(new []{Eol}, 0, _list, LengthOffset+sizeof(long), 1);

            _bytes += bytesToAdd;

            RewriteVariable(_list, BytesOffset, _bytes);
        }

        public byte[] GetBytes()
        {
            return _list;
        }

        public static ZipList FromBytes(byte[] bytes)
        {
        }

        public bool HasEntries
        {
            get { return _length > 0 && _tail > 0; }
        }

        public byte[] Pop()
        {

        }

        public void Push(byte[] bytes)
        {
            var lengthAdded = bytes.Length;
            ExtendBlockIfRequired(ref _list, (ulong)lengthAdded, _bytes);

            _length++;

            // _tail /*if 0 = offsets + length else += length*/
        }

        public void PushAll(byte[][] byteBlocks)
        {

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

        private static void RewriteVariable(byte[] block, int offset, ulong value)
        {
            var valueAsBits = BitConverter.GetBytes(value);

            for (var i = 0; i < sizeof(long); i++)
                block[offset + i] = valueAsBits[i];
        }


        private static void ExtendBlockIfRequired(ref byte[] block, ulong lengthModified, ulong offset)
        {
            var maxChangeLength = offset + lengthModified;
            if (maxChangeLength < (ulong)block.Length) return;

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
