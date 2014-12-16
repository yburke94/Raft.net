using System;
using System.IO;

namespace Raft.Infrastructure.IO
{
    public class BufferedSequentialFileWriter : IWriteToFile
    {
        public void CreateAndWrite(string filePath, byte[] data, long fileLength)
        {
            if (File.Exists(filePath))
                throw new InvalidOperationException(string.Format(
                    "File already exists at path: {0}. " +
                    "This method should only be called for creatong new files. " +
                    "To modify existing files, call BufferedSequentialFileWriter.Write().", filePath));

            using (var fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 2<<11, FileOptions.SequentialScan))
            {
                fs.SetLength(fileLength);
                fs.Write(data, 0, data.Length);
                fs.FlushProperly();
            }
        }

        public void Write(string filePath, long offset, byte[] data)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(string.Format(
                    "Could not find an existing file at path: {0}. " +
                    "This method is only for modifying existing files at " +
                    "the specified offset. If you need to create a file, " +
                    "consider calling BufferedSequentialFileWriter.CreateAndWrite() instead.", filePath));

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Write,
                    FileShare.None, 2 << 11, FileOptions.SequentialScan))
            {
                if ((offset + data.Length) > fs.Length)
                    throw new InvalidOperationException(string.Format(
                        "The file length would have been exceeded with the write. " +
                        "The current file length is set at {0}. To write all " +
                        "the data at the given offset, the file length would need to be {1}"
                        , fs.Length, (offset + data.Length)));

                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(data, 0, data.Length);
                fs.FlushProperly();
            }
        }
    }
}
