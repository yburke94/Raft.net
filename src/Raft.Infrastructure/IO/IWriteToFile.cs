using System;

namespace Raft.Infrastructure.IO
{
    public interface IWriteToFile
    {
        /// <summary>
        /// Writes the specified bytes to disk.
        /// </summary>
        /// <exception cref="InvalidOperationException">File does not exist</exception>
        void Write(string filePath, int offset, byte[] data);
    }
}
