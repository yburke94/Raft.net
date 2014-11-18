using System.IO;

namespace Raft.Infrastructure.IO
{
    public class BufferedSequentialFileWriter : IWriteToFile
    {
        private readonly int _fileSize;

        public BufferedSequentialFileWriter(int fileSize)
        {
            _fileSize = fileSize;
        }

        public void Write(string filePath, int offset, byte[] data)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate,
                FileAccess.Write, FileShare.None, 2 << 10, FileOptions.SequentialScan))
            {
                fs.SetLength(_fileSize);
                fs.FlushProperly();
            }
        }
    }

    //public class BufferedSequentialWriteToFileWriter : IWriteToFile
    //{
    //    public int Write(string filePath, byte[] data)
    //    {
    //        int bytesWritten = 0;

    //        using (var file = new FileStream(filePath, FileMode.OpenOrCreate,
    //            FileAccess.ReadWrite, FileShare.None, 2 << 10, FileOptions.SequentialScan))
    //        {
    //            file.SetLength(data.Length); // Need to pre-allocate.
    //            file.Write(data, 0, data.Length);

    //            file.FlushProperly();
    //        }

    //        return 0;
    //    }
    //}
}
