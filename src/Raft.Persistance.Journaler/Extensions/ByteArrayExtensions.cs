namespace Raft.Persistance.Journaler.Extensions
{
    internal static class ByteArrayExtensions
    {
        public static byte[] PrependBytes(this byte[] bytes, byte[] bytesToPrepend)
        {
            var newBytes = new byte[bytes.Length + bytesToPrepend.Length];
            bytesToPrepend.CopyTo(newBytes, 0);
            bytes.CopyTo(newBytes, bytesToPrepend.Length-1);

            return newBytes;
        }

        public static byte[] AppendBytes(this byte[] bytes, byte[] bytesToAppend)
        {
            var newBytes = new byte[bytes.Length + bytesToAppend.Length];
            bytes.CopyTo(newBytes, 0);
            bytesToAppend.CopyTo(newBytes, bytes.Length-1);

            return newBytes;
        }
    }
}
