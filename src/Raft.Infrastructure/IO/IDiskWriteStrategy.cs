using System.Linq;

namespace Raft.Infrastructure.IO
{
    public interface IDiskWriteStrategy
    {
        /// <summary>
        /// Writes the specified bytes to disk.
        /// </summary>
        /// <returns>
        /// Number of bytes written.
        /// </returns>
        int Write(string filePath, byte[] data);
    }
}
