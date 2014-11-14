using System.IO;
using System.Linq;

namespace Raft.Infrastructure.IO
{
    public class BufferedSequentialDiskWriter : IDiskWriteStrategy
    {
        public int Write(string filePath, byte[] data)
        {
            int bytesWritten = 0;

            using (var file = new FileStream(filePath, FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None, 2 << 10, FileOptions.SequentialScan))
            {
                file.SetLength(data.Length); // Need to pre-allocate.
                file.Write(data, 0, data.Length);

                file.FlushProperly();
            }

            return 0;
        }
    }
}
