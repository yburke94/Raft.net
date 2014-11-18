using System;
using System.IO;

namespace Raft.Infrastructure.IO
{
    public interface IWriteToFile
    {
        /// <summary>
        /// Creates a file with the given length and writes the specified bytes.
        /// </summary>
        void CreateAndWrite(string filePath, byte[] data, int fileLength);

        /// <summary>
        /// Writes the specified bytes to file at the given offset.
        /// </summary>
        /// <exception cref="FileNotFoundException">File does not exist</exception>
        /// <exception cref="InvalidOperationException">Writing bytes would exceed file length.</exception>
        void Write(string filePath, int offset, byte[] data);
    }
}
