using System;
using System.IO;

namespace Raft.Infrastructure.IO
{
    public class BufferedSequentialFileWriter : IWriteToFile
    {
        public void CreateAndWrite(string filePath, byte[] data, int fileLength)
        {
            if (File.Exists(filePath))
                throw new InvalidOperationException(string.Format(
                    "File already exists at path: {0}. " +
                    "This method should only be called for creatong new files. " +
                    "To modify existing files, call BufferedSequentialFileWriter.Write().", filePath));

            using (var fs = new FileStream(filePath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 2<<11, FileOptions.SequentialScan))
            {
                fs.SetLength(fileLength);
                fs.Write(data, 0, data.Length);
                fs.FlushProperly();
            }
        }

        public void Write(string filePath, int offset, byte[] data)
        {
            
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
